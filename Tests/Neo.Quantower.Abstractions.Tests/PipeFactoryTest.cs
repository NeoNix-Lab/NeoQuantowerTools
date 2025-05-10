using Moq;
using Neo.Quantower.Abstractions.Factories;
using Neo.Quantower.Abstractions.Interfaces;
using Neo.Quantower.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Quantower.Abstractions.Tests
{
    public class PipeFactoryTests
    {

        //[Fact]
        //public void Dispatcher_ShouldReturnNull_WhenNoDispatcherRegistered()
        //{
        //    // Arrange: reset implicito (mai chiamato RegisterDispatcher)

        //    // Act: provo ad accedere alla proprietà Dispatcher
        //    var result = PipeFactory.Dispatcher;

        //    // Assert: non essendoci un dispatcher registrato la factory ritorna null
        //    Assert.Null(result);
        //    // E lo status deve essere Requested, come impostato in EnsureInitialized()
        //    Assert.Equal(PipeDispatcherStatus.Requested, PipeFactory.Status);
        //}

        [Fact]
        public void RegisterDispatcher_ShouldSetIsInitializedFlag()
        {
            // Arrange: creo un mock di IPipeDispatcher che restituisce IsInitialized = true
            var mockDispatcher = new Mock<IPipeDispatcher>();
            mockDispatcher.SetupGet(d => d.IsInitialized).Returns(true);

            // Act: registro il dispatcher nella factory
            PipeFactory.RegisterDispatcher(mockDispatcher.Object);

            // Assert: la factory deve riflettere il flag IsInitialized
            Assert.True(PipeFactory.IsInitialized);
        }
    }
}
