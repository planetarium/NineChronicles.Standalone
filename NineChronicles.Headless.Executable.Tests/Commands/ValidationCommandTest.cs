using NineChronicles.Headless.Executable.Commands;
using Xunit;

namespace NineChronicles.Headless.Executable.Tests.Commands
{
    public class ValidationCommandTest
    {
        private readonly ValidationCommand _command;

        public ValidationCommandTest()
        {
            _command = new ValidationCommand();
        }

        [Theory]
        [InlineData("", -1)]
        [InlineData("invalid hexadecimal", -1)]
        [InlineData("0000000000000000000000000000000000000000000000000000000000000000", -1)]
        [InlineData("ab8d591ccdcce263c39eb1f353e44b64869f0afea2df643bf6839ebde650d244", 0)]
        [InlineData("d6c3e0d525dac340a132ae05aaa9f3e278d61b70d2b71326570e64aee249e566", 0)]
        [InlineData("761f68d68426549df5904395b5ca5bce64a3da759085d8565242db42a5a1b0b9", 0)]
        public void PrivateKey(string privateKeyHex, int exitCode)
        {
            Assert.Equal(exitCode, _command.PrivateKey(privateKeyHex));
        }
        
        [Theory]
        [InlineData("", -1)]
        [InlineData("invalid hexadecimal", -1)]
        [InlineData("000000000000000000000000000000000000000000000000000000000000000000", -1)]
        [InlineData("03b0868d9301b30c512d307ea67af4c8bef637ef099e39d32b808a43e6b41469c5", 0)]
        [InlineData("03308c1618a75e85a5fb57f7e453a642c307dc6310e90a7418b1aec565d963534a", 0)]
        [InlineData("028a6190bf643175b20e4a2d1d86fe6c4b8f7d5fe3d163632be4e59f83335824b8", 0)]
        public void PublicKey(string publicKeyHex, int exitCode)
        {
            Assert.Equal(exitCode, _command.PublicKey(publicKeyHex));
        }
    }
}
