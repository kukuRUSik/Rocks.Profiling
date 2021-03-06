﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using JetBrains.Annotations;
using Rocks.Profiling.Exceptions;
using Rocks.Profiling.Loggers;

namespace Rocks.Profiling.Models
{
    /// <summary>
    ///     Represents a stream of profile events.
    ///     This class is not thread safe.
    /// </summary>
    [DataContract]
    public sealed class ProfileSession
    {
        private readonly Stopwatch stopwatch;
        private readonly IProfilerLogger logger;
        private readonly List<ProfileOperation> operations;
        private readonly AsyncLocal<Stack<ProfileOperation>> operationsStack;

        private int newId;


        /// <exception cref="ArgumentNullException"><paramref name="profiler"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null" />.</exception>
        public ProfileSession([NotNull] IProfiler profiler,
                              [NotNull] IProfilerLogger logger)
        {
            this.stopwatch = Stopwatch.StartNew();

            this.Profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.operations = new List<ProfileOperation>();
            this.operationsStack = new AsyncLocal<Stack<ProfileOperation>>();
            this.Data = new Dictionary<string, object>(StringComparer.Ordinal);
        }


        /// <summary>
        ///     Profiler of the session.
        /// </summary>
        public IProfiler Profiler { get; }

        /// <summary>
        ///     Additional data associated with the session.
        /// </summary>
        [NotNull, DataMember(Name = "Data", EmitDefaultValue = false)]
        public IDictionary<string, object> Data { get; }

        /// <summary>
        ///     Gets or sets additional data for this session by key.
        ///     The key is case sensitive.
        /// </summary>
        /// <exception cref="ArgumentException" accessor="set">Argument <paramref name="dataKey" /> is null or empty</exception>
        public object this[[NotNull] string dataKey]
        {
            get
            {
                if (string.IsNullOrEmpty(dataKey))
                    throw new ArgumentException("Argument is null or empty", nameof(dataKey));

                object result;
                if (!this.Data.TryGetValue(dataKey, out result))
                    return null;

                return result;
            }

            set
            {
                if (string.IsNullOrEmpty(dataKey))
                    throw new ArgumentException("Argument is null or empty", nameof(dataKey));

                this.Data[dataKey] = value;
            }
        }

        /// <summary>
        ///     Current time in session.
        /// </summary>
        public TimeSpan Time => this.stopwatch.Elapsed;

        /// <summary>
        ///     Get the total duration of all operations in the session.
        /// </summary>
        [DataMember]
        public TimeSpan Duration { get; private set; }

        /// <summary>
        ///     The list of all operations in the session.
        /// </summary>
        [DataMember(Name = "Operations", EmitDefaultValue = false)]
        public IReadOnlyList<ProfileOperation> Operations => this.operations;

        /// <summary>
        ///     Returns true if there is an operation which <see cref="ProfileOperation.Duration" />
        ///     greater or equal to it's <see cref="ProfileOperation.NormalDuration" />.
        /// </summary>
        [DataMember]
        public bool HasOperationLongerThanNormal { get; private set; }


        /// <summary>
        ///     Adds new additional data to the <see cref="Data" /> dictionary.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="additionalData"/> is <see langword="null" />.</exception>
        public void AddData([NotNull] IDictionary<string, object> additionalData)
        {
            if (additionalData == null)
                throw new ArgumentNullException(nameof(additionalData));

            foreach (var kv in additionalData)
                this.Data[kv.Key] = kv.Value;
        }


        /// <summary>
        ///     Starts new operation measure.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="specification"/> is <see langword="null" />.</exception>
        [NotNull, MustUseReturnValue]
        internal ProfileOperation StartMeasure([NotNull] ProfileOperationSpecification specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            if (this.operationsStack.Value == null)
            {
                this.operationsStack.Value = new Stack<ProfileOperation>();
            }

            var operation_stack = this.operationsStack.Value;
            
            var last_operation = operation_stack.Count > 0 ? operation_stack.Peek() : null;

            this.newId++;

            var operation = new ProfileOperation(id: this.newId,
                                                 profiler: this.Profiler,
                                                 session: this,
                                                 specification: specification,
                                                 parent: last_operation);

            this.operations.Add(operation);
            operation_stack.Push(operation);

            return operation;
        }


        /// <summary>
        ///     Stops operation measure.
        ///     This method should not be called directly - it will be called automatically
        ///     uppon disposing of <see cref="ProfileOperation" /> returned from <see cref="StartMeasure" />.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <see langword="null" />.</exception>
        internal void StopMeasure([NotNull] ProfileOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            try
            {
                if (operation.Session != this)
                    throw new OperationFromAnotherSessionProfilingException();

                var operation_stack = this.operationsStack.Value;
                if (operation_stack == null)
                    throw new OperationsOutOfOrderProfillingException();
                
                var current_operation = operation_stack.Pop();
                if (current_operation != operation)
                    throw new OperationsOutOfOrderProfillingException();

                var parent_operation = operation_stack.Count > 0 ? operation_stack.Peek() : null;
                if (parent_operation != operation.Parent)
                    throw new OperationsOutOfOrderProfillingException();

                operation.EndTime = this.Time;

                this.Duration += operation.Duration;

                if (operation.Duration >= operation.NormalDuration)
                    this.HasOperationLongerThanNormal = true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex);
            }
        }
    }
}