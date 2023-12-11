#if NEMESIS_BINARY_PACKAGE_TESTS
using Nemesis.Essentials.Runtime;

namespace Nemesis.Essentials.Tests.Runtime;
#else
namespace Nemesis.Essentials.Sources.Tests.Runtime;
#endif

[TestFixture]
public class EventOfTests
{
    [Test]
    public void EventOf_ShouldReturnEvents()
    {
        var clickEvent = Event.Of<EventOfTests>(tit => tit.Click);
        var staticClickEvent = Event.Of<EventOfTests>(tit => StaticClick);
        Assert.Multiple(() =>
        {
            Assert.That(clickEvent, Is.EqualTo(GetType().GetEvent(nameof(Click))));
            Assert.That(staticClickEvent, Is.EqualTo(GetType().GetEvent(nameof(StaticClick))));
        });
    }

    public static event EventHandler StaticClick;
    protected static void OnStaticClick() => StaticClick?.Invoke(null, null);

    public event EventHandler Click;
    protected void OnClick() => Click?.Invoke(this, null);
}