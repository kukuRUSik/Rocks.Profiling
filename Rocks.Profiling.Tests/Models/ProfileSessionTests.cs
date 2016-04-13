﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Ploeh.AutoFixture;
using Rocks.Profiling.Models;
using Xunit;

namespace Rocks.Profiling.Tests.Models
{
    public class ProfileSessionTests
    {
        [Fact]
        public async Task StartAndStop_CorrectlySetsTheTimeOfTheRootOperation()
        {
            // arrange
            var fixture = new FixtureBuilder().Build();

            var sut = fixture.Create<ProfileSession>();


            // act
            using (sut.StartMeasure(new ProfileOperationSpecification("test")))
                await Task.Delay(100).ConfigureAwait(false);


            // assert
            sut.OperationsTreeRoot.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }
    }
}