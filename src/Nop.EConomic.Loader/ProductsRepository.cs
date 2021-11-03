using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Seo;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Installation;

namespace Nop.EConomic.Loader
{
    public class ProductsRepository : InstallationService
    {
        private readonly IDictionary<string, Product> _allProducts = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);

        private readonly INopDataProvider _dataProvider;
        private readonly ProductTemplate _simpleTemplate;
        private readonly int _carsCategoryId;

        public ProductsRepository(INopDataProvider dataProvider, INopFileProvider fileProvider, int category) : base(dataProvider, fileProvider,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            new EntRepository<ProductTag>(dataProvider),
            null, null, null, null, null, null, null,
            new EntRepository<UrlRecord>(dataProvider),
            null)
        {
            _dataProvider = dataProvider;

            _simpleTemplate = GetSingle<ProductTemplate>(pt => pt.Name == "Simple product");
            //_carsCategoryId = GetSingle<Category>(c => c.Name == "Cars").Id;
            _carsCategoryId = category;

            T GetSingle<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
            {
                var obj = dataProvider.GetTable<T>().FirstOrDefault(predicate);
                if (obj == null)
                    throw new Exception($"{typeof(T).Name} not found");
                return obj;
            }
        }

        public async Task AddProduct(ProductItem item)
        {
            var sku = GetSku(item);

            if (_allProducts.TryGetValue(sku, out var alreadyHandled) && alreadyHandled.UpdatedOnUtc == item.LastUpdated)
            {
                return;
            }

            var existing = _dataProvider.GetTable<Product>().FirstOrDefault(p => p.Sku == sku);

            var product = existing ?? new Product { Sku = sku, CreatedOnUtc = item.LastUpdated };
            ApplyProductData(product, item);
            _allProducts[sku] = product;

            if (existing != null)
            {
                await UpdateInstallationDataAsync(product);
                return;
            }

            await InsertInstallationDataAsync(product);

            await InsertInstallationDataAsync(new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = _carsCategoryId,
                DisplayOrder = 1,
            });

            await InsertInstallationDataAsync(new UrlRecord
            {
                EntityId = product.Id,
                EntityName = nameof(Product),
                LanguageId = 0,
                IsActive = true,
                Slug = await ValidateSeNameAsync(product, product.Name)
            });

            //await InsertInstallationDataAsync(new ProductManufacturer
            //{
            //    ProductId = product.Id,
            //    ManufacturerId = _dataProvider.GetTable<Manufacturer>().Single(m => m.Name == "...").Id,
            //    DisplayOrder = 1,
            //});

            // pictures ?

            var mapping = await InsertInstallationDataAsync(new ProductAttributeMapping
            {
                ProductId = product.Id,
                ProductAttributeId = 1, //_productAttributeRepository.Table.Single(x => x.Name == "Insurance").Id,
                AttributeControlType = AttributeControlType.RadioList,
                IsRequired = true
            });

            await InsertInstallationDataAsync(
                new ProductAttributeValue
                {
                    ProductAttributeMappingId = mapping.Id,
                    AttributeValueType = AttributeValueType.Simple,
                    Name = "Mega Protection",
                    IsPreSelected = true,
                    PriceAdjustment = 50,
                    DisplayOrder = 1
                },
                new ProductAttributeValue
                {
                    ProductAttributeMappingId = mapping.Id,
                    AttributeValueType = AttributeValueType.Simple,
                    Name = "Protection",
                    PriceAdjustment = 30,
                    DisplayOrder = 2
                },
                new ProductAttributeValue
                {
                    ProductAttributeMappingId = mapping.Id,
                    AttributeValueType = AttributeValueType.Simple,
                    Name = "Mini Protection",
                    PriceAdjustment = 15,
                    DisplayOrder = 3
                }
            );

            await AddProductTagAsync(product, "car");
        }

        private static string GetSku(ProductItem item) => "CAR_" + item.ProductNumber.Replace('"', '_'); // TODO ?

        private void ApplyProductData(Product prod, ProductItem data)
        {
            prod.ProductType = ProductType.SimpleProduct;
            prod.VisibleIndividually = true;
            prod.Name = data.Name;
            prod.ShortDescription = data.Description;
            prod.FullDescription = data.Description;
            prod.ProductTemplateId = _simpleTemplate.Id;
            prod.AllowCustomerReviews = true; // TODO ?
            prod.Price = Convert.ToDecimal(data.SalesPrice); // TODO which price to use ?

            // <?
            prod.IsShipEnabled = true;
            prod.IsFreeShipping = true;
            prod.Weight = 2;
            prod.Length = 2;
            prod.Width = 2;
            prod.Height = 2;
            //prod.TaxCategoryId = _taxCategoryRepository.Table.Single(tc => tc.Name == "Electronics & Software").Id;
            prod.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
            prod.StockQuantity = 10000;
            prod.NotifyAdminForQuantityBelow = 1;
            prod.AllowBackInStockSubscriptions = false;
            prod.DisplayStockAvailability = true;
            prod.LowStockActivity = LowStockActivity.DisableBuyButton;
            prod.BackorderMode = BackorderMode.NoBackorders;
            // ?>

            prod.OrderMinimumQuantity = 1;
            prod.OrderMaximumQuantity = 10000;
            prod.Published = true;

            // <?
            prod.ShowOnHomepage = true;
            prod.MarkAsNew = true;
            // ?>

            prod.UpdatedOnUtc = data.LastUpdated;
        }
    }
}
