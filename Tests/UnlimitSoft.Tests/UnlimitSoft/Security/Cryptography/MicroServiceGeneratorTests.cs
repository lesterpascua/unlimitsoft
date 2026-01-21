using System;
using System.Collections.Generic;
using System.Linq;
using UnlimitSoft.Security.Cryptography;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Security.Cryptography;


public sealed class MicroServiceGeneratorTests
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
        Assert.Equal(Convert.ToBase64String(global::UnlimitSoft.Security.Cryptography.Utility.GetNetworkInterface().GetPhysicalAddress().GetAddressBytes()), workerId);
    }

    #region GetServiceId
    [Fact]
    public void GetServiceId_MultipleGeneratedGuids_ShouldReturnSameServiceId()
    {
        // Arrange
        ushort expectedServiceId = 2000;
        var gen = new MicroServiceGenerator(expectedServiceId);

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            var generatedId = gen.GenerateId();
            var extractedServiceId = MicroServiceGenerator.GetServiceId(generatedId);
            Assert.Equal(expectedServiceId, extractedServiceId);
        }
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(255)]      // 8 bits: 11111111
    [InlineData(1023)]     // 10 bits: 1111111111
    [InlineData(16383)]    // 14 bits: 11111111111111
    [InlineData(65535)]    // 16 bits max: 1111111111111111 (full ushort range)
    [InlineData(0x3CFF)]   // Test with bits 8-9 set: 0011110011111111
    public void GetServiceId_FromGeneratedGuid_ShouldReturnCorrectServiceId(ushort expectedServiceId)
    {
        // Arrange
        var workerId = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
        var gen = new MicroServiceGenerator(expectedServiceId, workerId);

        // Act
        var generatedId = gen.GenerateId();
        var extractedServiceId = MicroServiceGenerator.GetServiceId(generatedId);

        // Assert - Full 16-bit serviceId is now preserved
        Assert.Equal(expectedServiceId, extractedServiceId);
    }
    #endregion

    #region GetWorkerId
    [Fact]
    public void GetWorkerId_MultipleGeneratedGuids_ShouldReturnSameWorkerId()
    {
        // Arrange
        var expectedWorkerId = new byte[] { 0xFF, 0xAA, 0x55, 0x00, 0x11, 0x22 };
        var gen = new MicroServiceGenerator(999, expectedWorkerId);

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            var generatedId = gen.GenerateId();
            var extractedWorkerId = MicroServiceGenerator.GetWorkerId(generatedId);

            Assert.Equal(expectedWorkerId[0], extractedWorkerId[0]);
            Assert.Equal(expectedWorkerId[1], extractedWorkerId[1]);
            Assert.Equal(expectedWorkerId[2], extractedWorkerId[2]);
            Assert.Equal(expectedWorkerId[3], extractedWorkerId[3]);
            Assert.Equal(expectedWorkerId[4], extractedWorkerId[4]);
        }
    }
    [Fact]
    public void GetWorkerId_FromGeneratedGuid_ShouldReturnCorrectWorkerIdBytes0To4()
    {
        // Arrange
        var expectedWorkerId = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC };
        var gen = new MicroServiceGenerator(1234, expectedWorkerId);

        // Act
        var generatedId = gen.GenerateId();
        var extractedWorkerId = MicroServiceGenerator.GetWorkerId(generatedId);

        // Assert - Should return bytes 0-4 (byte 5 is not preserved)
        Assert.Equal(5, extractedWorkerId.Length);
        Assert.Equal(expectedWorkerId[0], extractedWorkerId[0]); // _id0
        Assert.Equal(expectedWorkerId[1], extractedWorkerId[1]); // _id1
        Assert.Equal(expectedWorkerId[2], extractedWorkerId[2]); // _id2
        Assert.Equal(expectedWorkerId[3], extractedWorkerId[3]); // _id3
        Assert.Equal(expectedWorkerId[4], extractedWorkerId[4]); // _id4
    }
    #endregion
}
