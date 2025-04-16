using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Core;
public class MultilineScalarFlowStyleEmitter : ChainedEventEmitter
{
    public MultilineScalarFlowStyleEmitter(IEventEmitter nextEmitter)
        : base(nextEmitter) { }

    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        if (typeof(string).IsAssignableFrom(eventInfo.Source.Type))
        {
            string value = eventInfo.Source.Value as string;
            if (!string.IsNullOrEmpty(value))
            {
                bool isMultiLine = value.IndexOfAny(new char[] { '\r', '\n' }) >= 0;
                if (isMultiLine)
                {
                    // Remove leading whitespace but preserve intended indentation
                    var trimmed = value.TrimStart();
                    if (!trimmed.EndsWith("\n"))
                    {
                        trimmed += "\n";
                    }

                    // Emit scalar explicitly to prevent extra indentation indicators (like |2-)
                    var scalar = new YamlDotNet.Core.Events.Scalar(
                        anchor: null,
                        tag: null,
                        value: trimmed,
                        style: ScalarStyle.Literal,
                        isPlainImplicit: true,
                        isQuotedImplicit: false
                    );

                    emitter.Emit(scalar);
                    return; // prevent default behavior
                }
            }
        }

        nextEmitter.Emit(eventInfo, emitter);
    }
}