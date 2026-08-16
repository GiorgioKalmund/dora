using System;
using giorgiokalmund.Dora.Questing.Events;
using giorgiokalmund.Dora.Saving;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Questing
{
    public abstract class QuestStep<T> : AbstractQuestStep where T : struct, ISerializableData
    {
        [SerializeField] protected T currentState;

        /// <summary>
        /// Returns the initial / default state configuration.
        /// </summary>
        /// <remarks>Used to overwrite and reset the state.</remarks>
        protected abstract T GetInitialState();

        // Forward event for type safety
        protected override void ProcessEvent(IGameplayEvent e)
        {
            ProcessEvent(e, ref currentState);
        }

        protected virtual void ProcessEvent(IGameplayEvent e, ref T state)
        {
            
        }

        public virtual void OnSnapshotApplied(ISerializationProvider serializer, ref T state)
        {
            // intentionally left blank
        }
        
        public override ISerializableData GetSerializationState(ISerializationProvider _)
        {
            return currentState;
        }

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
            OnSnapshotApplied(serializer, ref currentState);

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

        /// <summary>
        /// Returns the Type of the state of the quest. Should in normally <i>not</i> be overriden.
        /// </summary>
        /// <returns></returns>
        public override Type GetStateType()
        {
            return typeof(T);
        }

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
        
    }
}