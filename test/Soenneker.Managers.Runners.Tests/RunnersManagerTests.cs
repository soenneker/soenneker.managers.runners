using Soenneker.Managers.Runners.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Managers.Runners.Tests;

[Collection("Collection")]
public class RunnersManagerTests : FixturedUnitTest
{
    private readonly IRunnersManager _util;

    public RunnersManagerTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IRunnersManager>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
