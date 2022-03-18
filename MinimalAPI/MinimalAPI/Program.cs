using Microsoft.EntityFrameworkCore;
using MinimalAPI.Data;
using MinimalAPI.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MinimalContextDb>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region CRUD
    app.MapGet("/fornecedor", async (
        MinimalContextDb db) =>
        await db.Fornecedores.ToListAsync())
        .WithName("GetFornecedor")
        .WithTags("Fornecedor");

    app.MapGet("/fornecedor/{id}", async (
        Guid id,
        MinimalContextDb db) =>
        await db.Fornecedores.FindAsync(id)
            is Fornecedor fornecedor
                ? Results.Ok(fornecedor)
                : Results.NotFound())
        .Produces<Fornecedor>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetFornecedorPorId")
        .WithTags("Fornecedor");

    app.MapPost("/fornecedor", async (
        Fornecedor fornecedor,
        MinimalContextDb db) =>
    {
        if (!MiniValidator.TryValidate(fornecedor, out var errors))
            return Results.ValidationProblem(errors);

        db.Fornecedores.Add(fornecedor);
        var result = await db.SaveChangesAsync();

        return result > 0
        //? Results.Created($"/fornecedor/{fornecedor.Id}", fornecedor)
        ? Results.CreatedAtRoute("GetFornecedorPorId", new { id = fornecedor.Id }, fornecedor)
        : Results.BadRequest("Houve um problema ao salvar o registro");
    })
        .ProducesValidationProblem()
        .Produces<Fornecedor>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithName("PostForncededor")
        .WithTags("Fornecedor");

    app.MapPut("/fornecedor/{id}", async (
        Guid id,
        Fornecedor fornecedor,
        MinimalContextDb db) =>
    {
        var fornecedorBanco = await db.Fornecedores.AsNoTracking<Fornecedor>().FirstOrDefaultAsync(f => f.Id.Equals(id));
        if (fornecedorBanco == null) return Results.NotFound();

        if (!MiniValidator.TryValidate(fornecedor, out var errors))
            return Results.ValidationProblem(errors);

        db.Fornecedores.Update(fornecedor);

        var result = await db.SaveChangesAsync();

        return result > 0
        ? Results.NoContent()
        : Results.BadRequest("Houve um problema ao salvar o registro");
    })
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .WithName("PutForncededor")
        .WithTags("Fornecedor");

    app.MapDelete("/fornecedor/{id}", async (
        Guid id,
        MinimalContextDb db) =>
    {
        var fornecedorBanco = await db.Fornecedores.FindAsync(id);
        if (fornecedorBanco == null) return Results.NotFound();

        db.Fornecedores.Remove(fornecedorBanco);

        var result = await db.SaveChangesAsync();

        return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Houve um problema ao salvar o registro");
    })
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("DeleteForncededor")
        .WithTags("Fornecedor");
#endregion

app.Run();