using HAL.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace HAL.Tests.AspNetCore;

[TestClass]
public class LinkFactoryTests
{
    [TestMethod]
    public void LinkFactoryGeneratesAsteriskForCollectionTypes()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var linkGeneratorMock = Substitute.For<LinkGenerator>();
        linkGeneratorMock
            .GetUriByAddress(httpContextAccessor.HttpContext, Arg.Any<RouteValuesAddress>(), Arg.Any<RouteValueDictionary>(), Arg.Any<RouteValueDictionary?>(), Arg.Any<string?>(), Arg.Any<HostString?>(), Arg.Any<PathString?>(), Arg.Any<FragmentString>(), Arg.Any<LinkOptions?>())
            .Returns("http://mockuri");
        var sut = new LinkFactory(linkGeneratorMock, httpContextAccessor, Substitute.For<IApiDescriptionGroupCollectionProvider>());

        // Act
        var link = sut.CreateTemplated("action", "controller", new { collection = null as int[] }, "http", "host");
        var href = link.Href;

        // Assert
        Assert.AreEqual("http://mockuri/{?collection*}", href);
    }
}
