using HAL.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HAL.Tests.AspNetCore
{
    [TestClass]
    public class LinkFactoryTests
    {
        [TestMethod]
        public void LinkFactoryGeneratesAsteriskForCollectionTypes()
        {
            // Arrange
            var actionContextAccessor = new ActionContextAccessor
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            var linkGeneratorMock = new Mock<LinkGenerator>();
            linkGeneratorMock
                .Setup(m => m.GetUriByAddress(actionContextAccessor.ActionContext.HttpContext, It.IsAny<RouteValuesAddress>(), It.IsAny<RouteValueDictionary>(), It.IsAny<RouteValueDictionary?>(), It.IsAny<string?>(), It.IsAny<HostString?>(), It.IsAny<PathString?>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions?>()))
                .Returns("http://mockuri");
            var sut = new LinkFactory(linkGeneratorMock.Object, actionContextAccessor, Mock.Of<IApiDescriptionGroupCollectionProvider>());

            // Act
            var link = sut.CreateTemplated("action", "controller", new { collection = null as int[] }, "http", "host");
            var href = link.Href;

            // Assert
            Assert.AreEqual("http://mockuri/{?collection*}", href);
        }
    }
}
