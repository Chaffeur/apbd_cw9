using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task DoSomethingAsync()
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        // BEGIN TRANSACTION
        try
        {
            command.CommandText = "INSERT INTO Animal VALUES (@IdAnimal, @Name);";
            command.Parameters.AddWithValue("@IdAnimal", 1);
            command.Parameters.AddWithValue("@Name", "Animal1");
        
            await command.ExecuteNonQueryAsync();
        
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO Animal VALUES (@IdAnimal, @Name);";
            command.Parameters.AddWithValue("@IdAnimal", 2);
            command.Parameters.AddWithValue("@Name", "Animal2");
        
            await command.ExecuteNonQueryAsync();
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        // END TRANSACTION
    }

    public async Task ProcedureAsync()
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "NazwaProcedury";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@Id", 2);
        
        await command.ExecuteNonQueryAsync();
        
    }
    public async Task<int> FulfillOrder(Product_Warehouse request)
    {
        using SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default"));
    await conn.OpenAsync();
    using SqlTransaction transaction = conn.BeginTransaction();

    try
    {
        
        var cmd = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @IdProduct", conn, transaction);
        cmd.Parameters.AddWithValue("@Idproduct", request.Idproduct);
        var exists = await cmd.ExecuteScalarAsync();
        if (exists == null)
            throw new ArgumentException("Product not found");

        
        cmd = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse", conn, transaction);
        cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        exists = await cmd.ExecuteScalarAsync();
        if (exists == null)
            throw new ArgumentException("Warehouse not found");

        
        cmd = new SqlCommand(@"
            SELECT TOP 1 IdOrder FROM [Order]
            WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt", conn, transaction);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
        cmd.Parameters.AddWithValue("@IdProduct", request.Idproduct);
        var orderIdObj = await cmd.ExecuteScalarAsync();
        if (orderIdObj == null)
            throw new ArgumentException("Matching order not found");

        int orderId = (int)orderIdObj;

        
        cmd = new SqlCommand("SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder", conn, transaction);
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        exists = await cmd.ExecuteScalarAsync();
        if (exists != null)
            throw new InvalidOperationException("Order already fulfilled");

        
        cmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = @Now WHERE IdOrder = @IdOrder", conn, transaction);
        cmd.Parameters.AddWithValue("@Now", DateTime.Now);
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        await cmd.ExecuteNonQueryAsync();

        
        cmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @IdProduct", conn, transaction);
        cmd.Parameters.AddWithValue("@IdProduct", request.Idproduct);
        var price = (decimal)await cmd.ExecuteScalarAsync();
        decimal totalPrice = price * request.Amount;

        
        cmd = new SqlCommand(@"
        INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
        VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
        SELECT SCOPE_IDENTITY();", conn, transaction);
        cmd.Parameters.AddWithValue("@Price", totalPrice);
        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
        cmd.Parameters.AddWithValue("@IdProduct", request.Idproduct);
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        
        var insertedIdObj = await cmd.ExecuteScalarAsync();
        int insertedId = Convert.ToInt32(insertedIdObj);
        
        transaction.Commit();
        return insertedId;
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
    }
    
}
