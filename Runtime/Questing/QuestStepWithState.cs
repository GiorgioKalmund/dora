using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Saving;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Questing
{
    public abstract class QuestStep<T> : AbstractQuestStep where T : struct, ISerializableData
    {
        #region State
        [SerializeField] protected T currentState;

        /// <summary>
        /// Returns the initial / default state configuration.
        /// </summary>
        /// <remarks>Used to overwrite and reset the state.</remarks>
        protected abstract T GetInitialState();
        
        /// <summary>
        /// Returns a copy of the State(<see cref="T"/>) to use for the serialization process.
        /// </summary>
        /// <param name="serializer">Reference to the serializer being used as part of the serialization process used to serialize the data.</param>
        /// <returns>The state to use.</returns>
        /// <remarks>If you want to modify the state (i.e. <see cref="currentState"/>) <b>BEFORE</b> it is serialized, this is the place.</remarks>
        public override ISerializableData GetSerializationState(ISerializationProvider serializer)
        {
            return currentState;
        }
        
        
        
        /// <summary>
        /// Disposes of the <see cref="currentState"/> and overwrites it with the incoming one.
        /// </summary>
        /// <param name="newState">The new state (<see cref="T"/>) to apply.</param>
        /// <remarks>If called during edit-time, will additionally mark the corresponding ScriptableObject as dirty to persistent the changes in the editor.</remarks>
        private void ApplyState(ref T newState)
        {
            // TODO: Maybe unmanaged resources? / Look more into disposing, should be fine for now for simple state objects
            currentState.Dispose();
            currentState = newState;
            
#if UNITY_EDITOR
            // Persist changes when working in the editor!
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// Allows modifying the <see cref="currentState"/> <b>AFTER</b> a <see cref="QuestStepSnapshot"/> has been applied to the quest step via <see cref="ApplyState"/>.
        /// </summary>
        /// <param name="serializer">Reference to the serializer being used as part of the serialization process used to deserialize the data.</param>
        /// <param name="state">A reference to the <see cref="currentState"/>. Can be safely modified.</param>
        public virtual void ModifyAppliedState(ISerializationProvider serializer, ref T state)
        {
            // intentionally left blank
        }
        
        /// <summary>
        /// Resets the current state. 
        /// </summary>
        /// <remarks>Base functionality simply overrides it by re-applying the state returned by <see cref="GetInitialState"/>.</remarks>
        public override void ResetState()
        {
            currentState = GetInitialState();
        }
        
        public override bool ProgressHasBeenMade()
        {
            return base.ProgressHasBeenMade();
            
            // TODO: We sadly cannot check for this, as the GetSerializationState post-pass interferes with this
            // || !currentState.Equals(GetInitialState());
        }
        
        #endregion

        #region Snapshots
        /// <summary>
        /// Applies a <see cref="QuestSnapshot"/> to step's <see cref="currentState"/>, as well as the completion flag.
        /// </summary>
        /// <param name="snapshot">The snapshot to apply.</param>
        /// <param name="serializer">The serializer used during the deserialization process of this quest step. Can be used to further deserialize nested data.</param>
        public override void ApplySnapshot(ref QuestStepSnapshot snapshot, ISerializationProvider serializer)
        {
            var newState = serializer.DeserializeData(snapshot.serializedData , typeof(T));
            //Debug.Log($"{ToString()} :\nIncoming:\t{newState}\nCurrent:\t{currentState}\t\n==> {newState.Equals(currentState)}\n(({IsCompleted}) -> ({snapshot.isCompleted}))==> {snapshot.isCompleted == IsCompleted}");

            // check incoming state is actually of the desired type
            if (newState is not T actualNewState)
            {
                DoraLogger.LogError($"{GetType()} has received invalid state object!\nExpected: <b>{typeof(T)}</b>\nGot: <b>{newState.GetType()}</b>\nDid you load an invalid save for the quest or quest step?");
                return;
            }
            
            // check for inconsistency in completion states
            if (!newState.Equals(currentState) && snapshot.isCompleted == IsCompleted)
            {
                DoraLogger.LogWarning($"Attempted to apply completed snapshot to completed step ('{name}') but the states differ!\n<i>Current State:</i>{currentState}\n<i>Snapshot State:/i>{newState}");
                return;
            }
            
            // if all caught up, simply return
            if (newState.Equals(currentState) && snapshot.isCompleted == IsCompleted)
            {
                //DoraLogger.Log($"State {GetType().Name} of step {name} is already all caught up. Nothing to apply.");
                return;
            }

            ApplyState(ref actualNewState);
            ModifyAppliedState(serializer, ref currentState);

            // Early return because after applying the new state the step has completed itself and 'IsCompleted' has updated.
            //
            // This is for example possible when the incoming state completes a child in a pool which then completed the pool.
            // If so no need for another TryComplete down below as it will have already been fired.
            if (snapshot.isCompleted == IsCompleted)
                return;
            
            // TODO: Here if we roll back to a state in a quest which is completed (for example rolling back to a location we are currently in)
            // TODO (cont): we might want to perform an additional check / force someone to fire an additional event?
            // TODO (cont) this ties in with stuff like accepting quests in a location you are already in and then insta-completing the step etc.

            if (snapshot.isCompleted)
            {
                // IsCompleted is set internally, do not change manually here
                TryComplete(); 
            }
            else
            {
                Update();
                IsCompleted = snapshot.isCompleted; // always 'false', just for control flow legibility
            }
        }
        
        #endregion

        #region IGameplayEvents
        /// <summary>
        /// Handles the forwarding to <see cref="ProcessEvent(IGameplayEvent,ref T)"/>.
        /// </summary>
        /// <param name="e">The incoming event.</param>
        /// <remarks>See <see cref="AbstractQuestStep.ProcessEvent"/>.</remarks>
        protected sealed override bool ProcessEvent(IGameplayEvent e)
        {
            return ProcessEvent(e, ref currentState);
        }

        /// <summary>
        /// Handles the processing of an <see cref="IGameplayEvent"/> with an additional reference to the <see cref="currentState"/>.
        /// </summary>
        /// <param name="e">The incoming event.</param>
        /// <param name="state">A reference to the <see cref="currentState"/>. Can be safely modified.</param>
        /// <remarks>Events passed into here are guarded by <see cref="AbstractQuestStep.CanProcess"/>.
        /// This might allow you to make some assumptions in regard to casting to specific event types. </remarks>
        /// <remarks>See <see cref="AbstractQuestStep.ProcessEvent"/>.</remarks>
        protected abstract bool ProcessEvent(IGameplayEvent e, ref T state);

        #endregion
    }
}