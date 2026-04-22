using Soenneker.Managers.Runners.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Managers.Runners.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class RunnersManagerTests : HostedUnitTest
{
    private readonly IRunnersManager _util;

    public RunnersManagerTests(Host host) : base(host)
    {
        _util = Resolve<IRunnersManager>(true);
    }

    [Test]
    public void Default()
    {

    }
}
