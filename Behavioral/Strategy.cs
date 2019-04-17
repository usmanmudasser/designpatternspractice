using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Reference:https://stackoverflow.com/questions/48732160/appropriate-design-pattern-for-the-payment-modules-c-sharp

//Interfaces
// Empty interface just to ensure that we get a compile
// error if we pass a model that does not belong to our
// payment system.
public interface IPaymentModel { }

public interface IPaymentService
{
    void MakePayment<T>(T model) where T : IPaymentModel;
    bool AppliesTo(Type provider);
}

public interface IPaymentStrategy
{
    void MakePayment<T>(T model) where T : IPaymentModel;
}
//Models
public class CreditCardModel : IPaymentModel
{
    public string CardHolderName { get; set; }
    public string CardNumber { get; set; }
    public int ExpirtationMonth { get; set; }
    public int ExpirationYear { get; set; }
}

public class PayPalModel : IPaymentModel
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

//Payment Service Abstraction
public abstract class PaymentService<TModel> : IPaymentService
    where TModel : IPaymentModel
{
    public virtual bool AppliesTo(Type provider)
    {
        return typeof(TModel).Equals(provider);
    }

    public void MakePayment<T>(T model) where T : IPaymentModel
    {
        MakePayment((TModel)(object)model);
    }

    protected abstract void MakePayment(TModel model);
}

//Payment Service Implementations
public class CreditCardPayment : PaymentService<CreditCardModel>
{
    protected override void MakePayment(CreditCardModel model)
    {
        //Implementation CreditCardPayment
        Console.WriteLine("credit card payment");
    }
}

public class PayPalPayment : PaymentService<PayPalModel>
{
    protected override void MakePayment(PayPalModel model)
    {
        //Implementation PayPalPayment
        Console.WriteLine("Paypal payment");
    }
}

//Payment Strategy
public class PaymentStrategy : IPaymentStrategy
{
    private readonly IEnumerable<IPaymentService> paymentServices;

    public PaymentStrategy(IEnumerable<IPaymentService> paymentServices)
    {
        if (paymentServices == null)
            throw new ArgumentNullException(nameof(paymentServices));
        this.paymentServices = paymentServices;
    }

    public void MakePayment<T>(T model) where T : IPaymentModel
    {
        GetPaymentService(model).MakePayment(model);
    }

    private IPaymentService GetPaymentService<T>(T model) where T : IPaymentModel
    {
        var result = paymentServices.FirstOrDefault(p => p.AppliesTo(model.GetType()));
        if (result == null)
        {
            throw new InvalidOperationException(
                $"Payment service for {model.GetType().ToString()} not registered.");
        }
        return result;
    }
}

//Usage
public class ClientStrategyPattern
{
    public static void Run()
    {
        // I am showing this in code, but you would normally 
        // do this with your DI container in your composition 
        // root, and the instance would be created by injecting 
        // it somewhere.
        var paymentStrategy = new PaymentStrategy(
            new IPaymentService[]
            {
        new CreditCardPayment(), // <-- inject any dependencies here
        new PayPalPayment()      // <-- inject any dependencies here
            });


        // Then once it is injected, you simply do this...
        var cc = new CreditCardModel() { CardHolderName = "Bob" };
        paymentStrategy.MakePayment(cc);

        // Or this...
        var pp = new PayPalModel() { UserName = "Bob" };
        paymentStrategy.MakePayment(pp);
    }
}