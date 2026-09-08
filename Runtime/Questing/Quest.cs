using System;
using System.Collections.Generic;
using System.Linq;
using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Saving;
using giorgiokalmund.Dora.Utils;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace giorgiokalmund.Dora.Questing
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Dora/Quest", order = 1)]
    public class Quest :  BaseComponent<QuestManager>, IEquatable<Quest>, IComparable<Quest>
    {
        #region Members & Properties

        [field: ReadOnly]
        [field: SerializeField, Tooltip("Cannot be recovered or completed. Can be set during every state except if already <see cref=\"QuestState.COMPLETED\"/>.")]
        [field: Header("Core")]
        public bool IsBotched { get; protected set; }
        
        [field: SerializeField, Tooltip("The state of the quest. Can only move forward. (Unless restarted / reset)")]
        [field: ReadOnly]
        public QuestPhase Phase { get; private set; }

        public bool IsCompleted => Phase == QuestPhase.COMPLETED;

        /// Whether the quest is botched or completed. This indicated that no operations which affect the quest are possible anymore.
        public bool IsBotchedOrCompleted => IsBotched || IsCompleted;

        [field: SerializeField, Tooltip("Information about the quest in general.")]
        public QuestInformation Information { get; protected set; }

        public virtual string StorageIdentifier => Information.identifier;

        [field: SerializeField, Tooltip("Optional rewards when the quest is completed.")]
        public RewardOption[] Rewards { get; protected set; }

        [field: SerializeField, Tooltip("Individual steps of the quest."), Expandable]
        public AbstractQuestStep[] Steps { get; protected set; }
        
        /// <summary>
        /// Represents the index of the current step if the quest is ACCEPTED, -1 otherwise.
        /// </summary>
        [SerializeField, Tooltip("The index of the current step. Is -1 if the quest is not currently accepted.")]
        private int currentStepIdx = -1;

        /// <inheritdoc cref="currentStepIdx"> </inheritdoc>
        public int CurrentStepIdx => currentStepIdx;
        
        [Header("Parent")]
        [Tooltip("The parent of the quest. If provided, Mentioning or Accepting a quest and a parent is given, it has to be Completed first!")]
        [SerializeField] private Quest parent;
        
        [Tooltip("Which action to perform if the parent is completed.")]
        [SerializeField] private ParentCompletionAction onParentCompleted = ParentCompletionAction.NONE;
        
        
        [CanBeNull]
        public AbstractQuestStep CurrentStep
        {
            get
            {
                if (Phase != QuestPhase.ACCEPTED || currentStepIdx < 0 || currentStepIdx >= Steps.Length)
                    return null;
                return Steps[currentStepIdx];
            }
        }

        internal IEnumerable<AbstractQuestStep> AllRequirementsToValidate => Steps.Where(r =>  !r?.SkipValidation ?? false);

        #endregion

        #region Events

        [Header("Quest Events")]
        public UnityEvent<QuestPhase> onPhaseChanged = new ();
        public UnityEvent<Quest> onUpdate = new();
        public UnityEvent<Quest> onAchieved = new();
        public UnityEvent<Quest> onComplete = new();
        public UnityEvent<Quest> onBotch = new();
        public UnityEvent<Quest> onReset = new();
        
        [Header("Step Events")]
        public UnityEvent<AbstractQuestStep> onStepStarted = new ();
        public UnityEvent<AbstractQuestStep> onStepUpdated = new ();
        public UnityEvent<AbstractQuestStep> onStepCompleted = new ();

        #endregion

        #region Validation

        public ValidationResult[] ValidateAllQuestSteps(bool isRuntime)
        {
            if (AllRequirementsToValidate == null)
                return Array.Empty<ValidationResult>();
            
            List<ValidationResult> failures = new List<ValidationResult>();
            foreach (var requirement in AllRequirementsToValidate)
            {
                var result = requirement.Validate(isRuntime);
                if (result.IsFailure)
                    failures.Add(result);
            }

            return failures.ToArray();
        }
        
        [NotNull]
        public ValidationResult ValidateQuestSteps(bool isRuntime)
        {
            if (AllRequirementsToValidate == null)
                return ValidationResult.Failure("There are no steps to validate!");
            
            foreach (var requirement in AllRequirementsToValidate)
            {
                var result = requirement.Validate(isRuntime);
                if (result.IsFailure)
                    return result;
            }

            return ValidationResult.Success();
        }

        [NotNull]
        public ValidationResult Validate(bool isRuntime)
        {
            int expectedCurrentIndex = -1;
            int completedCount = 0;
            for (var i = 0; i < Steps.Length; i++)
            {
                if (Steps[i].IsCompleted)
                {
                    expectedCurrentIndex = i + 1;
                    completedCount++;
                }
            }

            (Quest dependent1, Quest dependent2) = HasCircularDependency(new HashSet<Quest>() { this });
            Assert.IsTrue((dependent1 == null) == (dependent2 == null), "HasCircularDependency returned in invalid result!");
            if (dependent1 != null)
                return ValidationResult.Failure($"Has a circular dependency somewhere in connection with {dependent1.name} and {dependent2.name}.");

            if (parent && !parent.IsCompleted && (AnyProgress() || Phase != QuestPhase.UNKNOWN))
                return ValidationResult.Failure($"The parent quest ('{parent.name}') has not been completed yet. However, progress has been by either not being in 'UNKNOWN' phase, or a step having made progress already.");
            
            if (currentStepIdx != -1 && Phase != QuestPhase.ACCEPTED)
                return ValidationResult.Failure($"The step index is not what it should be. When not in the 'ACCEPTED' state it should be -1!. Is: {currentStepIdx}.");
            
            if (expectedCurrentIndex != currentStepIdx && Phase == QuestPhase.ACCEPTED)
                return ValidationResult.Failure($"Progress has been made to some quest steps but the step index says otherwise (CurrentStepIdx: {currentStepIdx}, Actual Completion Index: {expectedCurrentIndex}). This indicates some form of corruption or inconsistency. Please either resolve the issue manually or reset the quest.");
            
            if ((Phase < QuestPhase.ACCEPTED || currentStepIdx == 0) && AnyProgress())
                return ValidationResult.Failure($"Progress has been made to some quest steps but the state says otherwise ({Phase}). This indicates some form of corruption or inconsistency. Please either resolve the issue manually or reset the quest.");
            
            if (Phase == QuestPhase.COMPLETED && completedCount != Steps.Length)
                return ValidationResult.Failure("The quest is says it is completed but not all of its steps are completed...");

            return ValidateQuestSteps(isRuntime);
        }

        #region Tree Structure

        /// <summary>
        /// Used to determine whether the quest is at the root of a quest hierarchy.
        /// </summary>
        public bool IsRoot()
        {
            return parent == null;
        }


        /// <summary>
        /// Used to determine whether the hierarchy the quest is a part of contains a circular dependency.
        /// </summary>
        /// <remarks>
        /// These circles can be of any size, thus we must simply track the tree using 'seen' set.
        /// If a parent is already part of the set, we know there has to be a cycle somewhere in the graph.
        /// </remarks>
        public virtual (Quest, Quest) HasCircularDependency(HashSet<Quest> seen)
        {
            if (IsRoot())
                return (null, null);
            if (seen.Contains(parent))
                return (this, parent);

            seen.Add(this);
            return parent.HasCircularDependency(seen);
        }

        #endregion

        internal string[] GetInternalValidationResult()
        {
            List<string> errorMessages = new List<string>();
            if (String.IsNullOrEmpty(Information.identifier))
                errorMessages.Add("Identifier cannot be empty or null.");
            if (Steps == null)
                errorMessages.Add("Steps cannot be null.");
            else if (Steps.FirstOrDefault(s => s != null) == null)
                errorMessages.Add("Steps cannot be empty.");

            return errorMessages.ToArray();
        }

        #endregion

        #region Progress

        /// <summary>
        /// Returns the index / count of the current step, as well as the total amount of steps.
        /// Does <b>NOT</b> represent the correct logical index of the quest (see <see cref="CurrentStepIdx"/> for that).
        /// </summary>
        /// <remarks>The main use case of this function is for user-facing elements.</remarks>
        /// <example>
        /// Quest is Unknown / Mentioned -> (0, [X]);<br></br>
        /// Quest is Accepted -> ([IDX], [X]);<br></br>
        /// Quest is Achieved / Completed -> ([X], [X]);
        /// </example>
        public (int current, int total) GetStepProgress()
        {
            int currentStepVisual = Phase switch
            {
                < QuestPhase.ACCEPTED => 0,
                QuestPhase.ACCEPTED => currentStepIdx,
                > QuestPhase.ACCEPTED => Steps.Length
            };
            return (currentStepVisual, Steps.Length);
        }
        
        public bool TryNextStep(out AbstractQuestStep nextStep)
        {
            nextStep = null;
            if (IsBotchedOrCompleted)
            {
                DoraLogger.LogWarning("Cannot move onto next step. Quest botched or already completed.");
                return false;
            }

            if (Phase != QuestPhase.ACCEPTED)
            {
                DoraLogger.LogWarning("Cannot move onto next step. Quest not accepted yet.");
                return false;
            }
            
            if (Steps == null)
            {
                DoraLogger.LogWarning($"[{GetType()}]: Cannot advance to next step. There are no steps provided.");
                return false;
            };

            if (currentStepIdx >= Steps.Length)
            {
                DoraLogger.LogWarning("Cannot move onto next step. No more steps left.");
                return false;
            }

            if (CurrentStep != null)
            {
                if (!CurrentStep.IsCompleted)
                {
                    // In out-of-the-box experience of Dora this should never be invoked as NextStep is ONLY
                    // ever invoked when the current step has completed and fired its completion event.
                    // However, overrides of this method, or control flow changes through inheritance of this class
                    // might change this restricted calling and thus a separate check is required.
                    DoraLogger.LogWarning("Cannot move onto next step. Current step has not been completed yet.");
                    return false;
                }
                
                StepCompletedActions();
            }
            
            TryAdvanceStepIndex();

            nextStep = CurrentStep;
            return CurrentStep != null;
        }

        private void TryAdvanceStepIndex()
        {
            // hard stop at length of steps (aka. completion!)
            SetCurrentStep(Math.Min(currentStepIdx + 1, Steps.Length));
        }

        /// <summary>
        /// Sets the index of the current step and starts it if it is a valid index.
        /// </summary>
        /// <param name="index">The index of the step to set.</param>
        private void SetCurrentStep(int index)
        {
            currentStepIdx = index;
            if (CurrentStep != null)
                StepStartedActions();
            
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
            
            Update();
        }

        /// <returns></returns>
        /// <summary>
        /// Advances the phase based on the restricted flow of the state logic.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        protected internal bool TryAdvancePhase() => TryAdvancePhase(out _);
        
        /// <summary>
        /// Advances the phase based on the restricted flow of the phase logic.
        /// </summary>
        /// <param name="newPhase">The new <see cref="QuestPhase"/> after a successful operation.</param>
        /// <returns>Whether the operation was successful.</returns>
        protected internal bool TryAdvancePhase(out QuestPhase newPhase)
        {
            newPhase = Phase;

            var next = Phase.GetNext();
            if (next.HasValue)
            {
                newPhase = next.Value;
                return TrySetPhase(next.Value);
            }

            return false;
        }

        public bool Complete()
        {
            return TrySetPhase(QuestPhase.COMPLETED);
        }

        /// <summary>
        /// Attempts to set the state via <see cref="SetPhase"/>.
        /// The <see cref="newPhase"/> must come <b>AFTER</b> the current <see cref="Phase"/>.
        /// </summary>
        /// <param name="newPhase">The new state to set.</param>
        /// <returns>Whether setting the state to the new state was successful.</returns>
        protected internal bool TrySetPhase(QuestPhase newPhase)
        {
            if (IsBotched)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newPhase}' as it is botched.");
                return false;
            }
            
            if (IsCompleted)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newPhase}' as it already completed.");
                return false;
            }

            int phaseComp = newPhase.CompareTo(Phase);
            if (phaseComp == 0)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newPhase}' as it is already in that state.");
                return false;
            }
            
            if (phaseComp < 0)
            {
                DoraLogger.LogError($"Cannot set {Information} to state '{newPhase}' as it is already in further in state.'{Phase}'");
                return false;
            }

            // if we have a parent, ensure it is completed first!
            if (newPhase >= QuestPhase.MENTIONED && (!parent?.IsCompleted ?? false))
            {
                DoraLogger.LogWarning($"Cannot set {Information} to phase '{newPhase}' as its parent {parent.Information} is not completed.");
                return false;
            }

            if (newPhase >= QuestPhase.ACHIEVED && !CanBeAchieved())
            {
                DoraLogger.LogWarning($"Cannot set {Information} to phase '{newPhase}' as it cannot be achieved or completed right now.");
                return false;
            }

            return SetPhase(newPhase);
        }

        /// <summary>
        /// Almost fully unchecked setting the <see cref="Phase"/> of the quest.
        /// If it is the same as the current one no action is performed.
        /// </summary>
        /// <param name="newPhase">The new phase of the quest.</param>
        /// <param name="silent">Whether to emit related events when setting the new phase.</param>
        /// <returns>If the result of setting a new phase was successful.</returns>
        /// <remarks>For regular, consistent integration with your custom system please refer to <see cref="TrySetPhase"/>.</remarks>
        protected internal bool SetPhase(QuestPhase newPhase, bool silent = false)
        {
            QuestPhase oldPhase = Phase;
            if (newPhase == oldPhase)
            {
                //DoraLogger.Log($"Did not set new state. State of {Information} is already in '{State}'!");
                return false;
            }

            Phase = newPhase;
            onPhaseChanged.Invoke(Phase);
            Manager?.onQuestStateChanged.Invoke(this, Phase);
            
            // 
            // use 'newState' from here on out for logical legibility, however it would be equivalent to use the updated 'State' variable
            //

            // Only re-subscribe if coming from non-accepted state
            // If we already were in ACCEPTED, we handle the re-subscription via StepStartedActions in SetCurrentStep
            if (newPhase == QuestPhase.ACCEPTED)
            {
                if (!silent) 
                {
                    currentStepIdx = 0; // indicate the quest has started
                    Assert.IsNotNull(CurrentStep, $"Started the quest {Information} but the first step is null. This is not allowed! A quest must at least have one step if started during runtime.");
                    StepStartedActions();
                    Manager?.Register(this);
                }
                
                if (oldPhase == QuestPhase.COMPLETED) // Coming from completed
                    RetractCompletionRewards();
                if (oldPhase >= QuestPhase.ACCEPTED) // Coming from achieved or completed
                    RetractAchievedRewards();
            }

            if (newPhase == QuestPhase.ACHIEVED)
            {
                if (oldPhase < QuestPhase.ACHIEVED) // coming from anything below achieved
                    AchievedActions();
                else if (oldPhase > QuestPhase.ACHIEVED) // coming from completed
                {
                    RetractCompletionRewards();
                    return true;
                } 
                
                // If no reward demands to be handed in the dedicated 'ACHIEVED' state,
                // we simply move on to the completion state, which will hand out all anyways.
                if (Rewards == null || !Rewards.Any(r => r.handoutOnAchieved))
                {
                    // Should be true, as next state is 'COMPLETED'
                    Assert.IsTrue(TryAdvancePhase());
                    return true;
                }
            }

            if (newPhase == QuestPhase.COMPLETED)
            {
                CompletedActions();
                
                if (oldPhase != QuestPhase.ACHIEVED) // if we skip over "achieved" (i.e. on rollback) , we still need to hand out achieved rewards
                    HandOutAchievedRewards();
                HandOutCompletionRewards();
            }
            
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
            
            Update();

            return true;
        }

        /// <summary>
        /// Whether any quest step has already been completed.
        /// </summary>
        public bool AnyProgress()
        {
            return Steps.Any(s => s.ProgressHasBeenMade());
        }
        
        
        // TODO: This can probably removed and inlined
        private bool CanBeAchieved()
        {
            if (IsBotchedOrCompleted)
                return false;

            if (Steps == null)
                return false;

            if (Phase >= QuestPhase.ACHIEVED && currentStepIdx == -1)
                return true;

            // Check for equality here as in the final step completion we still call NextStep, which advances the index one last time
            if (currentStepIdx == Steps.Length && (CurrentStep?.IsCompleted ?? true))
                return true;

            return false;
        }

        
        /// <summary>
        /// Invokes all step-start related events.
        /// </summary>
        /// <remarks>As we always unsubscribe when the step is changed (even to itself), we do not guard the event firing here. On rollback this means that this is re-triggered, even if it's still the same step</remarks>
        private void StepStartedActions()
        {
            /* TODO: Duplicate subscription if new manager appears (for example if new scene with quest is (re)loaded!)
              We could either unsubscribe, or simply check if the step is started and do noop. Or even better not even invoke this function in that case.
            */
            Assert.IsNotNull(CurrentStep, $"Started the step for quest {Information} but the step is somehow null.");
            onStepStarted.Invoke(CurrentStep);
            CurrentStep.OnComplete.AddListener(HandleStepCompleted);
            CurrentStep.OnUpdated.AddListener(HandleCurrentStepUpdated);
        }

        /// <summary>
        /// Invokes all step-completion related events.
        /// </summary>
        /// <param name="isReset">Whether to only quietly remove the listeners to the current step. No completions event is fired if set. Used during rollback.</param>
        private void StepCompletedActions(bool isReset = false)
        {
            Assert.IsNotNull(CurrentStep, $"Completed the step of {Information} but the step is somehow null.");
            if (!isReset)
                onStepCompleted.Invoke(CurrentStep);
            CurrentStep.OnUpdated.RemoveListener(HandleCurrentStepUpdated);
            CurrentStep.OnComplete.RemoveListener(HandleStepCompleted);
        }
        
        /// <summary>
        /// Signals an update in the quest or any of its steps.
        /// </summary>
        public void Update()
        {
            onUpdate.Invoke(this);
        }

        /// <summary>
        /// Botches the quest.
        /// </summary>
        /// <returns>Whether botching was successful. For example, fails when it is already completed or botched.</returns>
        internal bool Botch()
        {
            if (IsBotchedOrCompleted)
            {
                DoraLogger.LogError($"Cannot botch quest {Information}. Already botched ({IsBotched}) or already completed ({IsCompleted})");
                return false;
            }

            BotchedActions();
            return true;
        }

        /// <summary>
        /// Internal actions and events related to botching the quest.
        /// </summary>
        private void BotchedActions()
        {
            IsBotched = true;
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
            onBotch.Invoke(this);
        }

        private void AchievedActions()
        {
            onAchieved.Invoke(this);
            
            HandOutAchievedRewards();
            SetCurrentStep(-1);
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// Internal actions and events related to completing the quest, such as handing out rewards.
        /// </summary>
        private void CompletedActions()
        {
            onComplete.Invoke(this);
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
        }

        #region Event Handling

        /// <summary>
        /// Handles the completion of a step based on <see cref="AbstractQuestStep.OnComplete"/>.
        /// If a current step exists, we either try advancing to the next one, or advance the quest's state.
        /// </summary>
        private void HandleStepCompleted()
        {
            if (!CurrentStep)
            {
                DoraLogger.LogError("The current step got completed but is null. Did you subscribe to completion more than once?");
                return;
            }

            if (!TryNextStep(out _))
            {
                Assert.IsTrue(Phase == QuestPhase.ACCEPTED, $"Quest {Information}: A step was completed but the quest is not in the 'ACCEPTED' state.");
                TryAdvancePhase(); // After accepted, either move on to ACHIEVED if rewards present, else completed
            }
        }
        
        private void HandleCurrentStepUpdated()
        {
            onStepUpdated.Invoke(CurrentStep);
        }

        #endregion

        #endregion

        #region Rewards

        internal void HandOutAchievedRewards()
        {
            if (Rewards == null)
                return;
            
            foreach (var reward in Rewards.Where(r => r.handoutOnAchieved))
                reward.rewards?.HandOut();
        }
        
        internal void RetractAchievedRewards()
        {
            if (Rewards == null)
                return;
            
            // reverse to maintain "stack ordering"
            foreach (var reward in Rewards.Where(r => r.handoutOnAchieved).Reverse())
                reward.rewards?.Retract();
        }

        internal void HandOutCompletionRewards()
        {
            if (Rewards == null)
                return;
            
            foreach (var reward in Rewards.Where(r => !r.handoutOnAchieved))
                reward.rewards.HandOut();
        }
        
        internal void RetractCompletionRewards()
        {
            if (Rewards == null)
                return;
            
            // reverse to maintain "stack ordering"
            foreach (var reward in Rewards.Where(r => !r.handoutOnAchieved).Reverse())
                reward.rewards.Retract();
        }
        
        #endregion

        #region IGameplayEvents

        internal void Process(IGameplayEvent e)
        {
            if (CurrentStep != null)
                CurrentStep.Process(e);
            else
            {
                // TODO: This case should not happen!
                //Debug.Log("no step can process the incoming event");
            }
        }

        #endregion

        #region IEquatable - based on QuestInformation

        public bool Equals(Quest other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Information, other.Information);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Quest)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Information);
        }

        #endregion

        #region IComparable - based on QuestInformation

        public int CompareTo(Quest other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return Information.CompareTo(other.Information);
        }

        #endregion

        #region Snapshots

        public QuestSnapshot CreateSnapshot(ISerializationProvider serializer)
        {
            int count;
            if (Phase >= QuestPhase.ACHIEVED) // capture all steps if achieved or completed
                count = Steps.Length;
            else if (Phase == QuestPhase.ACCEPTED) // capture only necessary if accepted
                count = currentStepIdx + 1;
            else count = 0; // capture none otherwise
            
            QuestStepSnapshot[] snapshots = new QuestStepSnapshot[count];

            // Only collect steps until the current step as future steps should have not been changed
            for (var i = 0; i < count; i++)
            {
                var snapshot = Steps[i].CreateSnapshot(serializer);
                snapshots[i] = snapshot;
            }
            
            return new QuestSnapshot()
            {
                currentStep = currentStepIdx,
                phase = Phase,
                isBotched = IsBotched,
                stepSnapshots = snapshots
            };
        }

        public void ApplySnapshot(ref QuestSnapshot snapshot, ISerializationProvider serializer)
        {
            //
            // Pre-verify some integrity of the snapshot
            //
            
            if (snapshot.phase > QuestPhase.COMPLETED)
            {
                DoraLogger.LogError("Error applying snapshot: Invalid state!");
                return;
            }
            
            if (snapshot.stepSnapshots.Length > Steps.Length)
            {
                DoraLogger.LogError($"Error applying snapshot: The snapshot contains more steps ({snapshot.stepSnapshots.Length}) than the quest ({Steps.Length})!");
                return;
            }
            
            if (snapshot.currentStep >= Steps.Length && snapshot.phase != QuestPhase.COMPLETED)
            {
                DoraLogger.LogError("Error applying snapshot: Invalid current step!");
                return;
            }

            // Ensure that the snapshot only contains steps which are completed
            // This is in line with the saving logic as all non-completed steps,
            // which are not the current step, are ignored by saving.
            // 
            // If we encounter a step which isn't completed, it must be the last one.
            bool firstNonCompletion = false;
            foreach (var snapshotStepSnapshot in snapshot.stepSnapshots)
            {
                if (firstNonCompletion)
                {
                    DoraLogger.LogError( "Error applying snapshot: Invalid serialized step layout!\n<b>Hint:</b> If loading encounters a step which isn't completed, it must be the last one. Your data might be incorrectly tampered with.");
                    return;
                }

                if (!snapshotStepSnapshot.isCompleted)
                    firstNonCompletion = true;
            }

            // To ensure Atomicity and Consistency, we roll back and abort on failure of any application of a snapshot
            // When applying new states, we first store the original state, if application fails, we roll back to the one stored in here.
            // If no errors occur, we simply discard. This method therefore does duplicate (worst case) the required memory, however it is only temporary as
            // it will be freed when the scope of this function ends. 
            //
            // The snapshots here should always be a sequential mirror of the state of the steps. With the last snapshot being the one of the 
            QuestStepSnapshot[] rollbackBuffer = new QuestStepSnapshot[Steps.Length];
            int rollbackIndex = -1;
            
            // Re-apply all steps by either grabbing their data from the snapshot,
            // or resetting them with the initial / base state + un-completion
            //
            // If only we would only override the steps which are part of the incoming snapshot
            // then steps which come later would keep their advanced state.
            // To save space we simply tell them to reset, instead of having to store a copy of their empty state. This should lead to the same result.
            for (int i = 0; i < Steps.Length; i++)
            {
                // If part of snapshot, apply snapshot entry
                if (i < snapshot.stepSnapshots.Length)
                {
                    // Collect all snapshots of previous, assumed to be valid (!!!) state. 
                    rollbackBuffer[i] = Steps[i].CreateSnapshot(serializer);
                    
                    // TODO: Explore if 'in' or 'ref readonly' (maybe available later Unity versions) as no write should be necessary
                    if (!Steps[i].ApplySnapshot(ref snapshot.stepSnapshots[i], serializer))
                    {
                        DoraLogger.LogError($"{name} could not apply incoming snapshot at position {i} (0-indexed). Rolling back to state before snapshot (assumed to be valid).\n");
                        rollbackIndex = i;
                        break;
                    };
                }
                else 
                    Steps[i].ResetStep();
            }
            
            if (rollbackIndex != -1)
            {
                // For the steps which require a rollback, we roll them back. 
                for (var i = 0; i < rollbackIndex + 1; i++)
                {
                    Steps[i].ApplySnapshot(ref rollbackBuffer[i], serializer);
                }
                
                // Early return, as we do not want to apply the rest of the snapshot
                return;
            }
            
            //
            // We can assume valid snapshot from here on out, as no steps need to be rolled back, and the validity of the properties has been checked beforehand
            //
            
            if (snapshot.phase == QuestPhase.ACCEPTED && Phase != QuestPhase.ACCEPTED)
                // We have to manually register the quest here as it was not registered at this point yet.
                Manager?.Register(this);
            
            if (CurrentStep) // CurrentStep only exists if quest hasn't been completed yet. If completed we cannot clean this up as it already was.
                StepCompletedActions(isReset:true); // First clean up all subscriptions to the current step 
            
            SetPhase(snapshot.phase, true);
            
            SetCurrentStep(snapshot.currentStep); // Then 'start' a new step, whether it is the same or an old one
            
            if (snapshot.isBotched)
                Botch();
        }
        
        #endregion
        
        #region Saving / Loading

        public bool Save(ISerializationProvider serializer, IStorageProvider storage)
        {
            if (string.IsNullOrEmpty(StorageIdentifier))
            {
                DoraLogger.LogError($"Cannot save quest {Information}. Identifier either null or empty! An identifier is required to store the quest.");
                return false;
            }
            
            var questData = serializer.SerializeData(CreateSnapshot(serializer));
            storage.Store(StorageIdentifier, questData);
            return true;
        }
        
        public bool Load(ISerializationProvider serializer, IStorageProvider storage)
        {
            if (!storage.TryLoad(StorageIdentifier, out string questData))
            {
                DoraLogger.LogWarning($"Cannot load quest {Information}. No matching entry in storage found for identifier: '{StorageIdentifier}'.");
                return false;
            }
            bool success = serializer.DeserializeData<QuestSnapshot>(questData, out var snapshot);
            if (!success)
            {
                DoraLogger.LogError($"Could not properly deserialize the data for '{StorageIdentifier}'. It might be malformatted or corrupted! No action was performed.");
                return false;
            }
            ApplySnapshot(ref snapshot, serializer);
            return true;
        }

        #endregion
        
        #region Lifecycle Integration
        
        private void Awake()
        {
            Information.Title = name;
        }
        
        /// <summary>
        /// Reset the quest and all of its steps (using <see cref="AbstractQuestStep.ResetStep"/> and <see cref="AbstractQuestStep.ResetState"/>).
        /// </summary>
        /// <remarks>Also works during runtime, all relevant events are emitted.</remarks>
        internal void ResetQuest()
        {
            if (CurrentStep)
                StepCompletedActions(isReset:true);
            
            Phase = QuestPhase.UNKNOWN;
            onPhaseChanged.Invoke(Phase);
            Manager?.onQuestStateChanged?.Invoke(this, Phase);
            onReset.Invoke(this);
            currentStepIdx = -1;
            IsBotched = false;
            if (Steps != null)
                foreach (var questStep in Steps)
                    questStep.ResetStep();
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
        }

        protected virtual void OnParentCompleted(Quest p)
        {
            Assert.IsNotNull(parent, $"The parent completion callback was triggered, but {Information} does not have a parent!");
            Assert.IsTrue(p.Equals(parent), $"The parent completion callback was triggered for {Information}, but the parent which triggered the completion ({p.Information}) is not the parent ({parent.Information})!");
            
            if (onParentCompleted == ParentCompletionAction.MENTION)
                TrySetPhase(QuestPhase.MENTIONED);
            else if (onParentCompleted == ParentCompletionAction.ACCEPT)
                TrySetPhase(QuestPhase.ACCEPTED);
            else
                DoraLogger.LogWarning("Unhandled case for parent completion logic!");
            
            parent.onComplete.RemoveListener(OnParentCompleted);
        }

        /// <summary>
        /// Callback invoked during the <b>Start</b> phase of the <see cref="QuestManager"/>'s lifecycle.
        /// </summary>
        public virtual void OnQuestManagerInit()
        {
            foreach (var questStep in Steps)
                questStep.OnQuestManagerInit();

            if (parent)
            {
                if (onParentCompleted != ParentCompletionAction.NONE)
                    parent.onComplete.AddListener(OnParentCompleted);
            }
            
            if (Phase == QuestPhase.ACCEPTED)
                Manager?.Register(this);
            
            // Init the quest before starting & prepare for gameplay
            if (CurrentStep != null)
                StepStartedActions();
        }

        /// <summary>
        /// Callback invoked during the <b>OnDestroy</b> phase of the <see cref="QuestManager"/>'s lifecycle.
        /// </summary>
        public virtual void OnQuestManagerDeinit()
        {
            foreach (var questStep in Steps)
                questStep.OnQuestManagerDeinit();
        }

        #endregion
    }
}