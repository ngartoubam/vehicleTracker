﻿using System;
using System.Threading;
using System.Threading.Tasks;

using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Snapshots;
using EventFlow.Snapshots.Strategies;

using VehicleTracker.Business.VehicleDomain.Events;

namespace VehicleTracker.Business.VehicleDomain {

    public class VehicleAggregate : SnapshotAggregateRoot<VehicleAggregate, VehicleId, VehicleSnapshot>{

        private readonly VehicleAggregateState _vehicleAggregateState = new VehicleAggregateState();

        public VehicleAggregate(VehicleId id) : base(id, SnapshotEveryFewVersionsStrategy.With(10)) {
            Register(_vehicleAggregateState);
        }


        #region MyRegion

        public IExecutionResult CreateVehicle(VehicleEntity vehicle) {
            Emit(new VehicleCreatedEvent(vehicle));
            return ExecutionResult.Success();
        }

        public IExecutionResult UpdateVehicleLocation(double latitude, double longitude, double zIndex) {
            Emit(new LocationUpdatedEvent(latitude, longitude, zIndex, DateTimeOffset.Now));
            return ExecutionResult.Success();
        }

        

        #endregion

        protected override Task<VehicleSnapshot> CreateSnapshotAsync(CancellationToken cancellationToken) {
            return Task.FromResult(new VehicleSnapshot(_vehicleAggregateState));
        }

        protected override Task LoadSnapshotAsync(VehicleSnapshot snapshot, ISnapshotMetadata metadata, CancellationToken cancellationToken) {

            _vehicleAggregateState.LoadSnapshot(snapshot);
            return Task.CompletedTask;
        }

    }
}