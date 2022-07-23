using UnlimitSoft.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Security.Cryptography
{
    public class MicroServiceGeneratorTests
    {
        [Fact]
        public void TakeXItemsConsecutiveInTime_VerifyNoDuplication_Success()
        {
            // Arrange
            var ids = new List<Guid>();
            var gen = new MicroServiceGenerator(2);


            // Act
            ids.AddRange(gen.Take(10000));


            // Assert
            Assert.DoesNotContain(ids.GroupBy(k => k), p => p.Count() != 1);
        }
        [Fact]
        public void BuildGenerator_SuppliedIdServiceAndIdWorker_ShouldMatchWhenRetrieve()
        {
            // Arrange
            var gen = new MicroServiceGenerator(2);


            // Act
            var workerId = gen.WorkerId;
            var serviceId = gen.ServiceId;


            // Assert
            Assert.Equal(2U, serviceId);
            Assert.Equal(Convert.ToBase64String(Utility.GetNetworkInterface().GetPhysicalAddress().GetAddressBytes()), workerId);
        }
    }
}
