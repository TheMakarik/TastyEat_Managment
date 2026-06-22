namespace TastyEat.Workstation.Models.Dto;

public sealed record ProductionEditDto
{
    public DateTime Date { get; set; }
    public List<ProductionItemEditDto> Items { get; set; } = [];
}

public sealed record ProductionItemEditDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public double Quantity { get; set; }
}
