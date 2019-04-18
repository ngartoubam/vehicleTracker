﻿using System;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Domain.Application.CommandServices;
using Domain.Application.QueryServices;
using Domain.Business.Vehicles;

using Microsoft.AspNetCore.Mvc;

using Vehicles.Service.ViewModels;

namespace Vehicles.Service.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class VehiclesController : Controller {
        private readonly IMapper _mapper;
        private readonly IVehicleQueryService _vehicleQueryService;
        private readonly IVehicleCommandService _vehicleCommandService;

        public VehiclesController(IVehicleQueryService vehicleQueryService, IVehicleCommandService vehicleCommandService, IMapper mapper) {
            _vehicleQueryService = vehicleQueryService;
            _vehicleCommandService = vehicleCommandService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicle(string id, CancellationToken cancellationToken) {
            if (string.IsNullOrEmpty(id))
                return BadRequest(nameof(NullReferenceException));


            var result = await _vehicleQueryService.GetVehicleByIdAsync(VehicleId.With(Guid.Parse(id)), cancellationToken);
            return new JsonResult(result);
        }


        [HttpPost]
        public async Task<IActionResult> StoreVehicle(VehicleViewModel vehicleViewModel,
            CancellationToken cancellationToken) {
            var vehicle = _mapper.Map<VehicleEntity>(vehicleViewModel);

            await _vehicleCommandService.CreateNewVehicleAsync(vehicle, cancellationToken);

            return Ok(vehicle);
        }
    }
}