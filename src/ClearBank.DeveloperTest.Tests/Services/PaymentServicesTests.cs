using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Services
{
    /*
     Note, if the code changed some of these unit tests could appear successful but negative results could be caused
     by different conditions. To solve this long term the response could return more information that could be used
     to confirm that the result or logging.

     With the orginal code all paths would have returned false, I've modified the behaviour so that happy paths can be
     returned
     */
    public class PaymentServicesTests
    {
        private Mock<IDataStore> _mockDataStore;

        public PaymentServicesTests()
        {
            _mockDataStore = new Mock<IDataStore>();
        }

        [Fact]
        public void MakePayment_NoAccountFound_ReturnsFalse()
        {
            //  Arrange
            Account account = null;
            _mockDataStore.Setup(m => m.GetAccount(It.IsAny<string>())).Returns(account); // Not necessary to mock but included for readability
            var paymentService = new PaymentService(_mockDataStore.Object);

            //  Act
            var result = paymentService.MakePayment(new MakePaymentRequest());

            //  Assert
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(PaymentScheme.FasterPayments)]
        [InlineData(PaymentScheme.Chaps)]
        [InlineData(PaymentScheme.Bacs)]
        public void MakePayment_MissingFlagForSchemeType_ReturnsFalse(PaymentScheme paymentScheme)
        {
            //  Arrange
            var account = new Account();
            _mockDataStore.Setup(m => m.GetAccount(It.IsAny<string>())).Returns(account);
            var paymentService = new PaymentService(_mockDataStore.Object);
            var request = new MakePaymentRequest
            {
                PaymentScheme = paymentScheme
            };

            //  Act
            var result = paymentService.MakePayment(request);

            //  Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void MakePayment_FasterPayments_BalanceBelowRequestAmount_ReturnsFalse()
        {
            //  Arrange
            var account = new Account 
            { 
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = 10 
            };

            _mockDataStore.Setup(m => m.GetAccount(It.IsAny<string>())).Returns(account);
            var paymentService = new PaymentService(_mockDataStore.Object);
            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.FasterPayments,
                Amount = 20
            };

            //  Act
            var result = paymentService.MakePayment(request);

            //  Assert
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(AccountStatus.Disabled)]
        [InlineData(AccountStatus.InboundPaymentsOnly)]
        public void MakePayment_Chaps_AmountStatusNotLive_ReturnsFalse(AccountStatus accountStatus)
        {
            //  Arrange
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Status = accountStatus
            };

            _mockDataStore.Setup(m => m.GetAccount(It.IsAny<string>())).Returns(account);
            var paymentService = new PaymentService(_mockDataStore.Object);
            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Chaps
            };

            //  Act
            var result = paymentService.MakePayment(request);

            //  Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void MakePayment_FasterPayments_SuccessfulPayment_ReturnsTrue()
        {
            //  Arrange
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = 20
            };

            _mockDataStore.Setup(m => m.GetAccount(It.IsAny<string>())).Returns(account);
            var paymentService = new PaymentService(_mockDataStore.Object);
            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.FasterPayments,
                Amount = 10
            };

            //  Act
            var result = paymentService.MakePayment(request);

            //  Assert
            Assert.True(result.Success);
            _mockDataStore.Verify(m => m.UpdateAccount(It.Is<Account>(p => p.Balance == 10)));
        }

        [Fact]
        public void MakePayment_Chaps_SuccessfulPayment_ReturnsTrue()
        {
            //  Arrange
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Status = AccountStatus.Live,
                Balance = 20
            };

            _mockDataStore.Setup(m => m.GetAccount(It.IsAny<string>())).Returns(account);
            var paymentService = new PaymentService(_mockDataStore.Object);
            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Chaps,
                Amount = 10
            };

            //  Act
            var result = paymentService.MakePayment(request);

            //  Assert
            Assert.True(result.Success);
            _mockDataStore.Verify(m => m.UpdateAccount(It.Is<Account>(p => p.Balance == 10)));
        }

        [Fact]
        public void MakePayment_Bacs_SuccessfulPayment_ReturnsTrue()
        {
            //  Arrange
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs,
                Balance = 20
            };

            _mockDataStore.Setup(m => m.GetAccount(It.IsAny<string>())).Returns(account);
            var paymentService = new PaymentService(_mockDataStore.Object);
            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Bacs,
                Amount = 10
            };

            //  Act
            var result = paymentService.MakePayment(request);

            //  Assert
            Assert.True(result.Success);
            _mockDataStore.Verify(m => m.UpdateAccount(It.Is<Account>(p => p.Balance == 10)));
        }

    }
}
