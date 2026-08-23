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
        public bool IsBotched { get; protected set; }
        
        [field: SerializeField, Tooltip("The state of the quest. Can only move forward. (Unless restarted / reset)")]
        [field: ReadOnly]
        public QuestState State { get; private set; }

        public bool IsCompleted => State == QuestState.COMPLETED;

        /// Whether the quest is botched or completed. This indicated that no operations which affect the quest are possible anymore.
        public bool IsBotchedOrCompleted => IsBotched || IsCompleted;

        [field: SerializeField, Tooltip("Information about the quest in general.")]
        public QuestInformation Information { get; protected set; }

        public virtual string StorageIdentifier => Information.identifier;

        [field: SerializeField, Tooltip("Optional initial Requirements for the quest to be met.")]
        [CanBeNull]
        public AbstractQuestStep BaseStep { get; protected set; }
        
        [field: SerializeField, Tooltip("Optional rewards when the quest is completed.")]
        [CanBeNull]
        public QuestRewards Rewards { get; protected set; }

        [field: SerializeField, Tooltip("Individual steps of the quest."), Expandable]
        public AbstractQuestStep[] Steps { get; protected set; }
        
        /// <summary>
        /// Represents the index of the current step if the quest is ACCEPTED, -1 otherwise.
        /// </summary>
        [SerializeField, Tooltip("The index of the current step. Is -1 if the quest is not currently accepted.")]
        private int currentStepIdx;

        /// <inheritdoc cref="currentStepIdx"> </inheritdoc>
        public int CurrentStepIdx => currentStepIdx;
        
        
        [CanBeNull]
        public AbstractQuestStep CurrentStep
        {
            get
            {
                if (State != QuestState.ACCEPTED || currentStepIdx < 0 || currentStepIdx >= Steps.Length)
                    return null;
                return Steps[currentStepIdx];
            }
        }

        internal IEnumerable<AbstractQuestStep> AllRequirementsToValidate => Steps.Where(r =>  !r?.SkipValidation ?? false);

        #endregion

        #region Events

        [Header("Quest Events")]
        public UnityEvent<QuestState> onStateChanged = new ();
        public UnityEvent<Quest> onUpdate = new();
        public UnityEvent<Quest> onComplete = new();
        public UnityEvent<Quest> onBotch = new();
        public UnityEvent<Quest> onReset = new();
        
        [Header("Step Events")]
        public UnityEvent<AbstractQuestStep> onStepStarted = new ();
        public UnityEvent<AbstractQuestStep> onStepUpdated = new ();
        public UnityEvent<AbstractQuestStep> onStepCompleted = new ();

        #endregion

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
            
            State = QuestState.UNKNOWN;
            onStateChanged.Invoke(State);
            Manager?.onQuestStateChanged?.Invoke(this, State);
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
            if (BaseStep)
            {
                var result = BaseStep.Validate(isRuntime);
                if (result.IsFailure)
                    return result;
            }

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
            
            if (currentStepIdx != -1 && State != QuestState.ACCEPTED)
                return ValidationResult.Failure($"The step index is not what it should be. When not in the 'ACCEPTED' state it should be -1!. Is: {currentStepIdx}.");
            
            if (expectedCurrentIndex != currentStepIdx && State == QuestState.ACCEPTED)
                return ValidationResult.Failure($"Progress has been made to some quest steps but the step index says otherwise (CurrentStepIdx: {currentStepIdx}, Actual Completion Index: {expectedCurrentIndex}). This indicates some form of corruption or inconsistency. Please either resolve the issue manually or reset the quest.");
            
            if ((State < QuestState.ACCEPTED || currentStepIdx == 0) && AnyQuestStepCompleted())
                return ValidationResult.Failure($"Progress has been made to some quest steps but the state says otherwise ({State}). This indicates some form of corruption or inconsistency. Please either resolve the issue manually or reset the quest.");
            
            if (State == QuestState.COMPLETED && completedCount != Steps.Length)
                return ValidationResult.Failure("The quest is says it is completed but not all of its steps are completed...");

            return ValidateQuestSteps(isRuntime);
        }

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
        
        protected bool TryNextStep(out AbstractQuestStep nextStep)
        {
            nextStep = null;
            if (IsBotchedOrCompleted)
            {
                DoraLogger.LogWarning("Cannot move onto next step. Quest botched or already completed.");
                return false;
            }

            if (State != QuestState.ACCEPTED)
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
        /// Advances the state based on the restricted flow of the state logic.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        internal bool TryAdvanceState() => TryAdvanceState(out _);
        
        /// <summary>
        /// Advances the state based on the restricted flow of the state logic.
        /// </summary>
        /// <param name="newState">The new <see cref="QuestState"/> after a successful operation.</param>
        /// <returns>Whether the operation was successful.</returns>
        internal bool TryAdvanceState(out QuestState newState)
        {
            newState = State;

            // State < QuestState.ACCEPTED ?
            if (State == QuestState.MENTIONED && (BaseStep && BaseStep.IsCompleted))
            {
                DoraLogger.LogWarning($"Cannot advance quest state {Information}. BaseStep is not completed yet.");
                return false;
            }

            var next = State.GetNext();
            if (next.HasValue)
            {
                newState = next.Value;
                return TrySetState(next.Value);
            }

            DoraLogger.LogWarning("no next state :(");
            return false;
        }

        /// <summary>
        /// Attempts to set the state via <see cref="SetState"/>.
        /// The <see cref="newState"/> must come <b>AFTER</b> the current <see cref="State"/>.
        /// </summary>
        /// <param name="newState">The new state to set.</param>
        /// <returns>Whether setting the state to the new state was successful.</returns>
        internal bool TrySetState(QuestState newState)
        {
            if (IsBotched)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newState}' as it is botched.");
                return false;
            }
            
            if (IsCompleted)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newState}' as it already completed.");
                return false;
            }

            int stateComp = newState.CompareTo(State);
            if (stateComp == 0)
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newState}' as it is already in that state.");
                return false;
            }
            
            if (stateComp < 0)
            {
                DoraLogger.LogError($"Cannot set {Information} to state '{newState}' as it is already in further in state.'{State}'");
                return false;
            }

            if (newState >= QuestState.ACHIEVED && !CanBeAchieved())
            {
                DoraLogger.LogWarning($"Cannot set {Information} to state '{newState}' as it cannot be achieved or completed right now.");
                return false;
            }

            return SetState(newState);
        }

        /// <summary>
        /// Almost fully unchecked setting the <see cref="State"/> of the quest.
        /// If it is the same as the current one no action is performed.
        /// </summary>
        /// <param name="newState">The new state of the quest.</param>
        /// <param name="silent">Whether to emit related events when setting the new state.</param>
        /// <returns>If the result of setting a new state was successful.</returns>
        /// <remarks>For regular, consistent integration with your custom system please refer to <see cref="TrySetState"/>.</remarks>
        private bool SetState(QuestState newState, bool silent = false)
        {
            if (newState == State)
            {
                //DoraLogger.Log($"Did not set new state. State of {Information} is already in '{State}'!");
                return false;
            }
            
            State = newState;
            
            onStateChanged.Invoke(State);
            Manager?.onQuestStateChanged.Invoke(this, State);
            
            // 
            // use 'newState' from here on out for logical legibility, however it would be equivalent to use the updated 'State' variable
            //

            // Only re-subscribe if coming from non-accepted state
            // If we already were in ACCEPTED, we handle the re-subscription via StepStartedActions in SetCurrentStep
            if (newState == QuestState.ACCEPTED && !silent)
            {
                currentStepIdx = 0; // indicate the quest has started
                Assert.IsNotNull(CurrentStep, $"Started the quest {Information} but the first step is null. This is not allowed! A quest must at least have one step if started during runtime.");
                StepStartedActions();
            }

            if (newState == QuestState.ACHIEVED && Rewards == null)
                return TryAdvanceState();

            if (newState == QuestState.COMPLETED && !silent)
                CompletedActions();
            
            
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
        public bool AnyQuestStepCompleted()
        {
            return Steps.Any(s => s.ProgressHasBeenMade());
        }
        
        
        private bool CanBeAchieved()
        {
            if (IsBotchedOrCompleted)
                return false;

            if (Steps == null)
                return false;

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
        
        /// <summary>
        /// Internal actions and events related to completing the quest, such as handing out rewards.
        /// </summary>
        private void CompletedActions()
        {
            onComplete.Invoke(this);
            HandOutRewards();
            // TODO: -1 should only indicate not started, completed is Steps.Length! CurrentStep logic also needs to change then!
            currentStepIdx = -1;
            
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
                Assert.IsTrue(State == QuestState.ACCEPTED, $"Quest {Information}: A step was completed but the quest is not in the 'ACCEPTED' state.");
                TryAdvanceState(); // After accepted, either move on to ACHIEVED if rewards present, else completed
            }
        }
        
        private void HandleCurrentStepUpdated()
        {
            onStepUpdated.Invoke(CurrentStep);
        }

        #endregion

        #endregion

        #region Rewards

        internal void HandOutRewards()
        {
            Rewards?.HandOut();
        }
        #endregion

        #region IGameplayEvents

        internal void Process(IGameplayEvent e)
        {
            if (CurrentStep != null)
                CurrentStep.Process(e);
            else
            {
                Debug.Log("no step can process the incoming event");
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
            if (State >= QuestState.ACHIEVED) // capture all steps if achieved or completed
                count = Steps.Length;
            else if (State == QuestState.ACCEPTED) // capture only necessary if accepted
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
                state = State,
                isBotched = IsBotched,
                stepSnapshots = snapshots
            };
        }

        public void ApplySnapshot(ref QuestSnapshot snapshot, ISerializationProvider serializer)
        {
            //
            // Pre-verify some integrity of the snapshot
            //
            
            if (snapshot.state > QuestState.COMPLETED)
            {
                DoraLogger.LogError("Error applying snapshot: Invalid state!");
                return;
            }
            
            if (snapshot.stepSnapshots.Length > Steps.Length)
            {
                DoraLogger.LogError($"Error applying snapshot: The snapshot contains more steps ({snapshot.stepSnapshots.Length}) than the quest ({Steps.Length})!");
                return;
            }
            
            if (snapshot.currentStep >= Steps.Length && snapshot.state != QuestState.COMPLETED)
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
            
            // TODO: Consistency / Atomicity -> if a snapshot fails, we still possibly have applied some anyways
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
                    // TODO: Explore if 'in' or 'ref readonly' (maybe available later Unity versions) as no write should be necessary
                    Steps[i].ApplySnapshot(ref snapshot.stepSnapshots[i], serializer);
                else 
                    Steps[i].ResetStep();
            }
            
            // TODO: Rollback here if atomicity can not be guaranteed
            
            if (snapshot.state == QuestState.ACCEPTED && State != QuestState.ACCEPTED)
                // We have to manually start the quest here as it was not registered at this point yet.
                Manager?.Register(this);
            
            if (CurrentStep) // CurrentStep only exists if quest hasn't been completed yet. If completed we cannot clean this up as it already was.
                StepCompletedActions(isReset:true); // First clean up all subscriptions to the current step 
            
            // TODO: Do we really want to re-emit events here + hand out rewards etc?
            SetState(snapshot.state, true);
            
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
            var snapshot = serializer.DeserializeData<QuestSnapshot>(questData);
            ApplySnapshot(ref snapshot, serializer);
            return true;
        }

        #endregion
        
        #region Lifecycle Integration

        /// <summary>
        /// Callback invoked during the <b>Start</b> phase of the <see cref="QuestManager"/>'s lifecycle.
        /// </summary>
        public virtual void OnQuestManagerInit()
        {
            foreach (var questStep in Steps)
                questStep.OnQuestManagerInit();
            
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