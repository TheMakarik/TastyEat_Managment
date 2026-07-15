using TastyEat.Workstation.Models.Tables;

namespace TastyEat.Workstation.Models.Dto;

public sealed record ClientOrderedProductDto(
    Product Product,
    int OrderedQuantity);
