using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IDbService
{
    Task DoSomethingAsync();
    Task ProcedureAsync();
    Task<int> FulfillOrder(Product_Warehouse request);
}