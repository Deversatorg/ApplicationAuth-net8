using Braintree;
using System;
using System.Linq;

namespace ApplicationAuth.Common.Extensions
{
    public static class BraintreeExtensions
    {
        public static void ThrowArgExOnOperationFailure(this Result<Customer> operationResult)
        {
            throw new ArgumentException(CreateErrorMessage(operationResult.Errors, operationResult.Message), "Braintree");
        }

        public static void ThrowArgExOnOperationFailure(this Result<Transaction> operationResult)
        {
            throw new ArgumentException(CreateErrorMessage(operationResult.Errors, operationResult.Message), "Braintree");
        }

        public static void ThrowArgExOnOperationFailure(this Result<Subscription> operationResult)
        {
            throw new ArgumentException(CreateErrorMessage(operationResult.Errors, operationResult.Message), "Braintree");
        }

        private static string CreateErrorMessage(ValidationErrors errors, string message)
        {
            string errorMessage = string.Join("\n", errors.DeepAll().Select(error => $"({(int)error.Code}) {error.Message}"));

            if (string.IsNullOrEmpty(errorMessage))
                errorMessage = message;

            return  errorMessage;
        }
    }
}
