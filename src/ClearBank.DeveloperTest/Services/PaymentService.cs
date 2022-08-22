using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    /*
        General comments
        I've omitted dependancy injection of the dataStore and logging

        With the orginal code all paths would have returned false, I've modified the behaviour so that happy paths can be
        returned
     */
    public class PaymentService : IPaymentService
    {
        private readonly IDataStore _dataStore;

        public PaymentService(IDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {

            Account account = _dataStore.GetAccount(request.DebtorAccountNumber);

            var result = GetValidationResult(request, account);
            
            if (result.Success)
            {
                account.Balance -= request.Amount;
                _dataStore.UpdateAccount(account);
            }

            return result;
        }

        private static MakePaymentResult GetValidationResult(MakePaymentRequest request, Account account)
        {
            var result = new MakePaymentResult { Success = true };

            if (account == null)
            {
                result.Success = false;
                return result;
            }

            if (!account.AllowedPaymentSchemes.HasFlag(PaymentSchemeToFlag(request.PaymentScheme)))
            {
                result.Success = false;
                return result;
            }

            switch (request.PaymentScheme)
            {
                case PaymentScheme.FasterPayments:
                    if (account.Balance < request.Amount)
                    {
                        result.Success = false;
                    }
                    break;

                case PaymentScheme.Chaps:
                    if (account.Status != AccountStatus.Live)
                    {
                        result.Success = false;
                    }
                    break;
            }

            return result;
        }

        private static AllowedPaymentSchemes PaymentSchemeToFlag(PaymentScheme paymentScheme)
        {
            return (AllowedPaymentSchemes)(1 << (int)paymentScheme);
        }
    }
}
