using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Pages;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Logging;
using ToggleCoreLibrary.Operations;
using Unleash;

namespace Microsoft.eShopWeb.Web.Services;

/// <summary>
/// This is a UI-specific service so belongs in UI project. It does not contain any business logic and works
/// with UI-specific types (view models and SelectListItem types).
/// </summary>
public class CatalogViewModelService : ICatalogViewModelService
{
    private readonly ILogger<CatalogViewModelService> _logger;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IRepository<CatalogBrand> _brandRepository;
    private readonly IRepository<CatalogType> _typeRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IUnleash _unleash;

    public CatalogViewModelService(
        ILoggerFactory loggerFactory,
        IRepository<CatalogItem> itemRepository,
        IRepository<CatalogBrand> brandRepository,
        IRepository<CatalogType> typeRepository,
        IUriComposer uriComposer,
        IUnleash unleash)
    {
        _logger = loggerFactory.CreateLogger<CatalogViewModelService>();
        _itemRepository = itemRepository;
        _brandRepository = brandRepository;
        _typeRepository = typeRepository;
        _uriComposer = uriComposer;
        _unleash = unleash;
    }

    public async Task<CatalogIndexViewModel> GetCatalogItems(int pageIndex, int itemsPage, int? brandId, int? typeId)
    {
        _logger.LogInformation("GetCatalogItems called.");

        var filterSpecification = new CatalogFilterSpecification(brandId, typeId);
        var filterPaginatedSpecification =
            new CatalogFilterPaginatedSpecification(itemsPage * pageIndex, itemsPage, brandId, typeId);

        // the implementation below using ForEach and Count. We need a List.
        var itemsOnPage = await _itemRepository.ListAsync(filterPaginatedSpecification);
        var totalItems = await _itemRepository.CountAsync(filterSpecification);

        //if (_unleash.IsEnabled("FT0001") && totalItems%6==0)
        //{
        //    return await NewRouteAsync(pageIndex, brandId, typeId);
        //}
        //if (_unleash.IsEnabled("FT0002") && totalItems <= 6)
        //{
        //    return await NewRouteAsync(pageIndex, 3, brandId, typeId);
        //}
        CatalogIndexViewModel vm = null;

        NewRouteAsync(ref vm, pageIndex, brandId, typeId);
        NewRouteAsync(ref vm, pageIndex, 3, brandId, typeId);

        if (vm == null) 
        {
            vm = new CatalogIndexViewModel()
            {
                CatalogItems = itemsOnPage.Select(i => new CatalogItemViewModel()
                {
                    Id = i.Id,
                    Name = i.Name,
                    PictureUri = _uriComposer.ComposePicUri(i.PictureUri),
                    Price = i.Price
                }).ToList(),
                Brands = (GetBrands()).ToList(),
                Types = (GetTypes()).ToList(),
                BrandFilterApplied = brandId ?? 0,
                TypesFilterApplied = typeId ?? 0,
                PaginationInfo = new PaginationInfoViewModel()
                {
                    ActualPage = pageIndex,
                    ItemsPerPage = itemsOnPage.Count,
                    TotalItems = totalItems,
                    TotalPages = int.Parse(Math.Ceiling(((decimal)totalItems / itemsPage)).ToString())
                }
            };

            vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
            vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";
        }
        return vm;
    }

    [FeatureToggle("FT0002")]
    private void NewRouteAsync(ref CatalogIndexViewModel vm, int pageIndex, int itemsPage, int? brandId, int? typeId)
    {
        _logger.LogInformation("GetCatalogItems called.");

        var filterSpecification = new CatalogFilterSpecification(brandId, typeId);
        var filterPaginatedSpecification =
            new CatalogFilterPaginatedSpecification(itemsPage * pageIndex, itemsPage, brandId, typeId);

        // the implementation below using ForEach and Count. We need a List.
        var task1 = _itemRepository.ListAsync(filterPaginatedSpecification);
        var task2 = _itemRepository.CountAsync(filterSpecification);
        task1.Wait();
        task2.Wait();
        var itemsOnPage = task1.Result;
        var totalItems = task2.Result;

        if (totalItems % 6 != 0)
            return;

        vm = new CatalogIndexViewModel()
        {
            CatalogItems = itemsOnPage.Select(i => new CatalogItemViewModel()
            {
                Id = i.Id,
                Name = i.Name,
                PictureUri = _uriComposer.ComposePicUri(i.PictureUri),
                Price = i.Price
            }).ToList(),
            Brands = ( GetBrands()).ToList(),
            Types = (GetTypes()).ToList(),
            BrandFilterApplied = brandId ?? 0,
            TypesFilterApplied = typeId ?? 0,
            PaginationInfo = new PaginationInfoViewModel()
            {
                ActualPage = pageIndex,
                ItemsPerPage = itemsOnPage.Count,
                TotalItems = totalItems,
                TotalPages = int.Parse(Math.Ceiling(((decimal)totalItems / itemsPage)).ToString())
            }
        };

        vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
        vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";
    }

    [FeatureToggle("FT0001")]
    private void NewRouteAsync(ref CatalogIndexViewModel vm, int pageIndex, int? brandId, int? typeId)
    {
        _logger.LogInformation("GetCatalogItems called.");

        int itemsPage = 6;
        var filterSpecification = new CatalogFilterSpecification(brandId, typeId);
        var filterPaginatedSpecification =
            new CatalogFilterPaginatedSpecification(itemsPage * pageIndex, itemsPage, brandId, typeId);

        // the implementation below using ForEach and Count. We need a List.
        var task1 = _itemRepository.ListAsync(filterPaginatedSpecification);
        var task2 = _itemRepository.CountAsync(filterSpecification);
        task1.Wait();
        task2.Wait();
        var itemsOnPage = task1.Result;
        var totalItems = task2.Result;

        if (totalItems > 6)
            return;

        vm = new CatalogIndexViewModel()
        {
            CatalogItems = itemsOnPage.Select(i => new CatalogItemViewModel()
            {
                Id = i.Id,
                Name = i.Name,
                PictureUri = _uriComposer.ComposePicUri(i.PictureUri),
                Price = i.Price
            }).ToList(),
            Brands = (GetBrands()).ToList(),
            Types = (GetTypes()).ToList(),
            BrandFilterApplied = brandId ?? 0,
            TypesFilterApplied = typeId ?? 0,
            PaginationInfo = new PaginationInfoViewModel()
            {
                ActualPage = pageIndex,
                ItemsPerPage = itemsOnPage.Count,
                TotalItems = totalItems,
                TotalPages = int.Parse(Math.Ceiling(((decimal)totalItems / itemsPage)).ToString())
            }
        };

        vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
        vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";
    }

    public IEnumerable<SelectListItem> GetBrands()
    {
        _logger.LogInformation("GetBrands called.");
        var task = _brandRepository.ListAsync();
        task.Wait();
        var brands = task.Result;

        var items = brands
            .Select(brand => new SelectListItem() { Value = brand.Id.ToString(), Text = brand.Brand })
            .OrderBy(b => b.Text)
            .ToList();

        var allItem = new SelectListItem() { Value = null, Text = "All", Selected = true };
        items.Insert(0, allItem);

        return items;
    }

    public IEnumerable<SelectListItem> GetTypes()
    {
        _logger.LogInformation("GetTypes called.");
        var task = _typeRepository.ListAsync();
        task.Wait();
        var types = task.Result;

        var items = types
            .Select(type => new SelectListItem() { Value = type.Id.ToString(), Text = type.Type })
            .OrderBy(t => t.Text)
            .ToList();

        var allItem = new SelectListItem() { Value = null, Text = "All", Selected = true };
        items.Insert(0, allItem);

        return items;
    }
}
