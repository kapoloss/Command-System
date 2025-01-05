using System.Collections.Generic;
using CommandSystem.Sequences;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommandSystem
{
    /// <summary>
    /// Manages a collection of command sequences and provides functionality to add and manage sequences.
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        /// <summary>
        /// A list of sequences managed by the CommandManager.
        /// </summary>
        [FormerlySerializedAs("Sequences")] [SerializeField] public List<Sequence> sequences = new List<Sequence>();

        /// <summary>
        /// Adds a new sequence to the manager.
        /// </summary>
        /// <param name="sequence">The sequence to add.</param>
        public void AddSequence(Sequence sequence)
        {
            if (sequence == null)
            {
                throw new System.ArgumentNullException(nameof(sequence), "Cannot add a null sequence.");
            }

            sequences.Add(sequence);
        }
    }
}