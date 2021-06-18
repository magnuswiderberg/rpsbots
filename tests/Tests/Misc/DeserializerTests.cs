using System.Text.Json;
using System.Text.Json.Serialization;
using SharedKernel.ApiModels_V1;
using Xunit;

namespace Tests.Misc
{
    public class DeserializerTests
    {
        [Fact]
        public void HandShape_Can_Be_Deserialized()
        {
            var json = $"{{ \"Shape\": \"{Shape.paper}\" }}";
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            var hand = JsonSerializer.Deserialize<HandShape>(json, options);
            Assert.Equal(Shape.paper, hand.Shape);
        }
    }
}
