using System;
using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Saving;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora.Questing
{
    [Serializable]
    public abstract class AbstractQuestStep : ScriptableObject 
    {
        #region Members & Properties

        [field: SerializeField, Tooltip("Whether to skip internal static validation.")]
        public bool SkipValidation { get; protected set; }
        
        [field: SerializeField, Tooltip("Whether this step has been completed")]
        [field: ReadOnly]
        public bool IsCompleted { get; protected set; }

        /// <summary>
        /// Whether it is possible for the requirements to be achieved during edit time.
        /// </summary>
        public bool CanBeAchievedAtEditTime => HandleValidation(false).IsSuccess;
        
        /// <summary>
        /// Whether it is possible for the requirements to be achieved during runtime.
        /// </summary>
        public bool CanBeAchievedAtRuntime => HandleValidation(true).IsSuccess;

        public bool CanBeCompleted => CheckCompletion() && !IsCompleted;
        #endregion

        #region Events

        [NotNull] internal UnityEvent OnComplete = new ();
        [NotNull] internal UnityEvent OnUpdated = new ();

        #endregion

        #region Validation

        /// <summary>
        /// Returns the static validation result of the quest step.
        /// </summary>
        /// <returns>
        /// Should return a new <see cref="ValidationResult.Failure"/> if something has gone wrong,
        /// else a <see cref="ValidationResult.Success"/>.
        /// </returns>
        [NotNull] protected abstract ValidationResult HandleValidation(bool isRuntime);

        /// <summary>
        /// Validates the internals using <see cref="HandleValidation"/> if internal static validation is not skipped (<see cref="SkipValidation"/>).
        /// </summary>
        /// <returns></returns>
        [NotNull] internal ValidationResult Validate(bool isRuntime)
        {
            if (!SkipValidation)
                return HandleValidation(isRuntime);
            return ValidationResult.Success();
        }
        
        #endregion

        #region Completion
        
        /// <summary>
        /// Returns whether the current state of the quest step allows completion of the step.
        /// </summary>
        /// <remarks>Override this method to create your custom, detailed checks.</remarks>
        protected virtual bool CheckCompletion() { return true; }

        /// <summary>
        /// If <see cref="CanBeCompleted"/> / <see cref="CheckCompletion"/>, will <see cref="Complete"/> the step,
        /// otherwise returns false.
        /// </summary>
        protected bool TryComplete()
        {
            bool result = CanBeCompleted;
            if (result)
                Complete();
            return result;
        }
        
        /// <summary>
        /// Completes the quest and invokes <see cref="OnComplete"/>.
        /// </summary>
        /// <remarks>Idempotent. Event is only fired exactly once (unless <see cref="ResetStep"/> of course).</remarks>
        private void Complete()
        {
            if (IsCompleted)
            {
                DoraLogger.LogWarning($"Cannot complete quest step {ToString()}. Already completed");
                return;
            }
            
            IsCompleted = true;
            OnComplete.Invoke();
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
        }
        
        #endregion

        #region Updating, Progress & Restting


        protected void Update()
        {
            OnUpdated.Invoke();
        }

        public void ResetStep()
        {
            IsCompleted = false;
            ResetState();
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// Resets the quest requirement to its starting state. All variables which track progress should be reset.
        /// </summary>
        public abstract void ResetState();
        
        /// <summary>
        /// Determines whether any type of progress, either via change in the state, or completion has been made. 
        /// </summary>
        public virtual bool ProgressHasBeenMade()
        {
            return IsCompleted;
        }
        
        #endregion

        #region Snapshots & Serialization
        
        /// <summary>
        /// Applies a <see cref="QuestSnapshot"/> to step, as well as the completion flag.
        /// </summary>
        /// <param name="snapshot">The snapshot to apply.</param>
        /// <param name="serializer">The serializer used during the deserialization process of this quest step. Can be used to further deserialize nested data.</param>
        /// <remarks>Left abstract at this point in time as <see cref="QuestStep{T}"/> creates the actual logical foundation for the use of this concept.</remarks>
        public abstract bool ApplySnapshot(ref QuestStepSnapshot snapshot, ISerializationProvider serializer);
        
        
        /// <summary>
        /// Creates a new <see cref="QuestStepSnapshot"/> based on the current state of the step.
        /// </summary>
        /// <param name="serializer">The <see cref="ISerializationProvider"/> to use to serialize all related data.</param>
        public QuestStepSnapshot CreateSnapshot(ISerializationProvider serializer)
        {
            ISerializableData currentState = GetSerializationState(serializer);
            return new QuestStepSnapshot()
            {
                serializedData = serializer.SerializeData(currentState),
                isCompleted = IsCompleted
            };
        }
        
        /// <summary>
        /// Returns the state object used to serialize and store.
        /// </summary>
        /// <remarks>Overriding this can be used to inject custom data which is not tracked by the state into it for serialization.</remarks>
        public abstract ISerializableData GetSerializationState(ISerializationProvider serializer);

        #endregion

        #region Display

        /// <summary>
        /// Returns a user-friendly description of the step based on its current internal state.
        /// </summary>
        public abstract string GetDescription();
        
        // TODO: Maybe differentiate between editor description and gameplay description?

        #endregion

        #region IGameplayEvents

        /// <summary>
        /// Attempts to process an incoming <see cref="IGameplayEvent"/> if it can be processed (<see cref="CanProcess"/>).
        /// </summary>
        /// <param name="e">The incoming <see cref="IGameplayEvent"/>.</param>
        internal bool Process(IGameplayEvent e)
        {
            if (IsCompleted)
            {
                DoraLogger.LogWarning($"The quest step '{name}' has received a '{e.GetType().Name}' event even though it is already marked as completed.");
                return false;
            }

            // rely on short-circuit evaluation!
            return CanProcess(e) && ProcessEvent(e);
        }

        /// <summary>
        /// Guards whether the type or contents of the incoming <see cref="IGameplayEvent"/> should be passed onto <see cref="ProcessEvent"/>.
        /// </summary>
        /// <param name="e">The incoming <see cref="IGameplayEvent"/>.</param>
        protected abstract bool CanProcess(IGameplayEvent e);
        
        /// <summary>
        /// Processes any <see cref="IGameplayEvent"/> which has passed the <see cref="CanProcess"/> check.
        /// </summary>
        /// <param name="e">The incoming <see cref="IGameplayEvent"/></param>.
        protected abstract bool ProcessEvent(IGameplayEvent e);

        #endregion

        #region Lifecycle Integration

        /// <inheritdoc cref="Quest.OnQuestManagerInit"> </inheritdoc>
        public virtual void OnQuestManagerInit()
        {
            // Intentionally left blank
        }

        /// <inheritdoc cref="Quest.OnQuestManagerDeinit"> </inheritdoc>
        public virtual void OnQuestManagerDeinit()
        {
            // Intentionally left blank
        }

        #endregion
    }
}