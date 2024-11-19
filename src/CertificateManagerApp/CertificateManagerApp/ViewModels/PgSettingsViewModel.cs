using CertificateManagerApp.Models;
using CertificateManagerApp.Services;
using CertificateManagerApp.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CertificateManagerApp.ViewModels;

public partial class PgSettingsViewModel : ObservableObject
{
    readonly IIdentityServices identityServ;

    public PgSettingsViewModel(IIdentityServices identityServices)
    {
        identityServ = identityServices;
    }

    [RelayCommand]
    async Task GoToBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    #region IDENTIDAD
    [ObservableProperty]
    ObservableCollection<Owner>? owners;

    [ObservableProperty]
    Owner? selectedOwner;

    [ObservableProperty]
    string? commonName;

    [ObservableProperty]
    string? organization;

    [ObservableProperty]
    string? country;

    [ObservableProperty]
    string? email;

    [ObservableProperty]
    bool isVisibleInfo;

    [RelayCommand]
    async Task AddOwner()
    {
        IsVisibleInfo = string.IsNullOrEmpty(CommonName) || string.IsNullOrEmpty(Organization) || string.IsNullOrEmpty(Country);

        if (IsVisibleInfo)
        {
            await Task.Delay(4000);
            IsVisibleInfo = false;
            return;
        }

        Identity newIdentity = new()
        {
            CommonName = CommonName!.Trim().ToUpper(),
            Organization = Organization!.Trim().ToUpper(),
            Country = Country!.Trim().ToUpper(),
            Email = Email?.Trim(),
        };
        var result = identityServ.Insert(newIdentity);
        if (string.IsNullOrEmpty(result))
        {
            return;
        }

        Owners ??= [];

        Owner newOwner = new() { CommonName = CommonName, Id = result };
        Owners.Insert(0, newOwner);

        WeakReferenceMessenger.Default.Send(true.ToString(), "39579BB1B80F4B1F9B8D1AB2CAEEB5AB");

        CommonName = null;
        Organization = null;
        Country = null;
        Email = null;
    }

    [RelayCommand]
    void RemoveOwner()
    {
        var deleteOwner = SelectedOwner!;
        var result = identityServ.Delete(deleteOwner.Id!);
        if (result)
        {
            Owners!.Remove(deleteOwner);
            SelectedOwner = null;

            if (Owners.Count == 0)
            {
                WeakReferenceMessenger.Default.Send(false.ToString(), "39579BB1B80F4B1F9B8D1AB2CAEEB5AB");
            }
        }
    }
    #endregion

    public async void Initialize()
    {
        if (identityServ.Exist)
        {
            var ownersAll = identityServ.GetAll();
            if (ownersAll is null || ownersAll.Count() == 0)
            {
                return;
            }
            Owners = [.. ownersAll.Select(x => x.ToOwner()).OrderBy(x => x.CommonName)];
        }
        
        await Task.CompletedTask;
    }
}
