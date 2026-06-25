using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SIMSProject.Enums;
using SIMSProject.Models;

namespace SIMSProject.Wpf;

public partial class MainWindow : Window
{
    private readonly AppServices _services = new();
    private readonly Dictionary<string, Button> _navButtons = new();
    private User? _currentUser;
    private ContentControl? _content;
    private TextBlock? _message;
    private TextBox? _searchBox;

    public MainWindow()
    {
        InitializeComponent();
        PreviewKeyDown += HandleGlobalShortcuts;
        ShowLogin();
    }

    private void ShowLogin()
    {
        _currentUser = null;
        Root.Children.Clear();

        var page = new Grid { Margin = new Thickness(48) };
        page.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
        page.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var intro = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(20) };
        intro.Children.Add(HeroBlock(
            "SIMS",
            "Pametan pregled zgrada, stanova i zahteva",
            "Prijavite se, izaberite ulogu i nastavite kroz jasne kartice i akcije."));
        intro.Children.Add(InfoStrip("Brzi tok", "Admin dodaje zgradu, upravnik je odobrava, stanar salje zahtev za stan."));
        Grid.SetColumn(intro, 0);
        page.Children.Add(intro);

        var loginCard = Card();
        loginCard.Width = 390;
        loginCard.VerticalAlignment = VerticalAlignment.Center;
        loginCard.HorizontalAlignment = HorizontalAlignment.Center;
        var form = new StackPanel();
        form.Children.Add(Heading("Dobrodosli", 28));
        form.Children.Add(Text("Unesite email i lozinku. Sistem sam prepoznaje ulogu korisnika.", 13, "#64748B"));
        var email = Input("Email");
        var password = Password("Lozinka");
        _message = Message();
        form.Children.Add(Field("Email", email));
        form.Children.Add(Field("Lozinka", password));
        form.Children.Add(_message);

        var loginButton = Button("Login", "PrimaryButton");
        loginButton.IsDefault = true;
        loginButton.Click += (_, _) =>
        {
            SetMessage("");
            var user = _services.Auth.Login(new LoginCredentials
            {
                Email = email.Text.Trim(),
                Password = password.Password
            });

            if (user is null)
            {
                SetMessage("Email ili lozinka nisu ispravni.", true);
                return;
            }

            _currentUser = user;
            ShowShell();
        };

        var registerButton = Button("Registracija stanara", "SecondaryButton");
        registerButton.Click += (_, _) => ShowTenantRegistration();
        form.Children.Add(loginButton);
        form.Children.Add(registerButton);
        loginCard.Child = form;
        Grid.SetColumn(loginCard, 1);
        page.Children.Add(loginCard);
        Root.Children.Add(page);
    }

    private void ShowTenantRegistration()
    {
        Root.Children.Clear();
        var page = CenteredPage("Registracija stanara", "Popunite osnovne podatke. Email i lozinka moraju biti jedinstveni.");
        var form = (StackPanel)((Border)page.Children[0]).Child;
        var jmbg = Input("JMBG");
        var email = Input("Email");
        var password = Password("Lozinka");
        var firstName = Input("Ime");
        var lastName = Input("Prezime");
        var phone = Input("Mobilni telefon");
        _message = Message();
        AddAll(form, FormGrid(
            Field("JMBG", jmbg),
            Field("Email", email),
            Field("Lozinka", password),
            Field("Ime", firstName),
            Field("Prezime", lastName),
            Field("Mobilni telefon", phone)), _message);

        var actions = Row();
        var save = Button("Registruj se", "PrimaryButton");
        save.IsDefault = true;
        save.Click += (_, _) =>
        {
            SetMessage("");
            if (!RequireFields(jmbg, email, firstName, lastName, phone) || string.IsNullOrWhiteSpace(password.Password))
            {
                SetMessage("Popunite sva polja.", true);
                return;
            }

            Try(() =>
            {
                _services.Tenants.RegisterTenant(new Tenant
                {
                    Jmbg = jmbg.Text.Trim(),
                    Email = email.Text.Trim(),
                    Password = password.Password,
                    FirstName = firstName.Text.Trim(),
                    LastName = lastName.Text.Trim(),
                    MobilePhone = phone.Text.Trim()
                });
                SetMessage("Stanar je uspesno registrovan.");
                Clear(jmbg, email, firstName, lastName, phone);
                password.Clear();
            });
        };

        var back = Button("Nazad na login", "SecondaryButton");
        back.IsCancel = true;
        back.Click += (_, _) => ShowLogin();
        var clearForm = Button("Ocisti formu", "SecondaryButton");
        clearForm.Click += (_, _) =>
        {
            Clear(jmbg, email, firstName, lastName, phone);
            password.Clear();
            SetMessage("");
        };
        actions.Children.Add(save);
        actions.Children.Add(clearForm);
        actions.Children.Add(back);
        form.Children.Add(actions);
        Root.Children.Add(page);
    }

    private void ShowShell()
    {
        if (_currentUser is null)
        {
            ShowLogin();
            return;
        }

        Root.Children.Clear();
        _navButtons.Clear();
        var shell = new Grid();
        shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.RowDefinitions.Add(new RowDefinition());
        shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        shell.ColumnDefinitions.Add(new ColumnDefinition());

        var top = new Grid { Background = Brush("#FFFFFF"), Height = 76, Margin = new Thickness(0, 0, 0, 1) };
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        top.ColumnDefinitions.Add(new ColumnDefinition());
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        top.Children.Add(new Border { Background = Brush("#007CC2") });

        var identity = Stack(
            Heading("SIMS Buildings", 22),
            Text($"{_currentUser.FirstName} {_currentUser.LastName} - {RoleName(_currentUser)}", 13, "#52677A"));
        identity.Margin = new Thickness(18, 10, 0, 0);
        Grid.SetColumn(identity, 1);
        top.Children.Add(identity);

        var logout = Button("Logout", "DangerButton");
        logout.Margin = new Thickness(0, 12, 20, 12);
        logout.MinWidth = 132;
        logout.Click += (_, _) => ShowLogin();
        Grid.SetColumn(logout, 2);
        top.Children.Add(logout);
        Grid.SetColumnSpan(top, 2);
        shell.Children.Add(top);

        var nav = new StackPanel { Background = Brush("#123C69"), Margin = new Thickness(0), VerticalAlignment = VerticalAlignment.Stretch };
        nav.Children.Add(NavButton("Dashboard", ShowDashboard));
        nav.Children.Add(NavButton("Zgrade", ShowBuildings));
        if (_currentUser is Tenant)
        {
            nav.Children.Add(NavButton("Moji zahtevi", ShowTenantRequests));
            nav.Children.Add(NavButton("Podnesi zahtev", ShowTenantNewRequest));
        }
        else if (_currentUser is BuildingManager)
        {
            nav.Children.Add(NavButton("Moje zgrade", ShowManagerBuildings));
            nav.Children.Add(NavButton("Zahtevi", ShowManagerRequests));
            nav.Children.Add(NavButton("Dodaj stan", ShowAddApartment));
        }
        else if (_currentUser is Administrator)
        {
            nav.Children.Add(NavButton("Dodaj upravnika", ShowAdminAddManager));
            nav.Children.Add(NavButton("Dodaj zgradu", ShowAdminAddBuilding));
        }

        Grid.SetRow(nav, 1);
        shell.Children.Add(nav);

        _content = new ContentControl { Margin = new Thickness(24) };
        Grid.SetRow(_content, 1);
        Grid.SetColumn(_content, 1);
        shell.Children.Add(_content);
        Root.Children.Add(shell);
        ShowDashboard();
    }

    private void ShowDashboard()
    {
        SetActiveNav("Dashboard");
        var panel = Page(
            "Dashboard",
            "Izaberite karticu za posao koji zelite da zavrsite.",
            "Zgrade su centralni pregled.",
            "Akcije ispod su prilagodjene vasoj ulozi.",
            "Nakon svake izmene liste se osvezavaju.");
        var cards = Wrap();
        cards.Children.Add(ActionCard("Zgrade", "Pretraga, filteri, sortiranje i pregled kartica.", "#0F766E", ShowBuildings));
        if (_currentUser is Tenant)
        {
            cards.Children.Add(ActionCard("Moji zahtevi", "Pregled, filter i povlacenje zahteva.", "#2563EB", ShowTenantRequests));
            cards.Children.Add(ActionCard("Podnesi zahtev", "Posaljite zahtev za pristup zgradi i stanu.", "#EA580C", ShowTenantNewRequest));
        }
        else if (_currentUser is BuildingManager)
        {
            cards.Children.Add(ActionCard("Moje zgrade", "Odobrite ili odbijte zgrade na cekanju.", "#0F766E", ShowManagerBuildings));
            cards.Children.Add(ActionCard("Zahtevi", "Izaberite zgradu i obradite zahteve.", "#2563EB", ShowManagerRequests));
            cards.Children.Add(ActionCard("Dodaj stan", "Dodajte stan u svoju odobrenu zgradu.", "#EA580C", ShowAddApartment));
        }
        else if (_currentUser is Administrator)
        {
            cards.Children.Add(ActionCard("Dodaj upravnika", "Registrujte upravnika i odmah ga koristite za zgrade.", "#2563EB", ShowAdminAddManager));
            cards.Children.Add(ActionCard("Dodaj zgradu", "Unesite adresu, lokaciju i JMBG upravnika.", "#EA580C", ShowAdminAddBuilding));
        }

        panel.Children.Add(cards);
        SetContent(panel);
    }

    private void ShowBuildings()
    {
        SetActiveNav("Zgrade");
        var panel = Page(
            "Zgrade",
            "Pretrazite odobrene zgrade kao katalog. JMBG upravnika se ne prikazuje.",
            "Unesite ulicu, naselje, grad ili sifru zgrade.",
            "Napredni filteri pokrivaju spratove i stanove.",
            "Sortiranje po spratovima je dostupno jednim klikom.");
        var filter = Card();
        filter.Padding = new Thickness(24);

        var quickSearch = Input("npr. Liman, Bulevar, Grbavica");
        _searchBox = quickSearch;
        quickSearch.MinHeight = 58;
        quickSearch.FontSize = 16;
        var field = Combo("Bez dodatnog filtera", "Adresa", "Naselje", "Broj spratova", "Broj soba", "Broj stanara", "Sobe & stanari", "Sobe | stanari");
        var query = Input("Vrednost");
        var room = Input("Broj soba");
        var tenants = Input("Broj stanara");
        var sort = new CheckBox { Content = "Sortiraj po spratovima", Margin = new Thickness(0, 14, 18, 0), VerticalAlignment = VerticalAlignment.Center };
        var apply = Button("Primeni", "PrimaryButton");
        apply.IsDefault = true;
        apply.Content = "Pretrazi";
        apply.MinWidth = 170;
        apply.MinHeight = 58;
        apply.VerticalAlignment = VerticalAlignment.Bottom;
        apply.Margin = new Thickness(14, 0, 0, 10);

        var searchRow = new Grid { Margin = new Thickness(0, 8, 0, 14) };
        searchRow.ColumnDefinitions.Add(new ColumnDefinition());
        searchRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var quickField = Field("Brza pretraga", quickSearch);
        Grid.SetColumn(quickField, 0);
        searchRow.Children.Add(quickField);
        Grid.SetColumn(apply, 1);
        searchRow.Children.Add(apply);

        var advancedGrid = new UniformGrid { Columns = 4 };
        AddAll(advancedGrid,
            Field("Dodatni filter", field),
            Field("Vrednost", query),
            Field("Broj soba", room),
            Field("Broj stanara", tenants));
        var advancedPanel = new Border
        {
            Background = Brush("#F8FBFC"),
            BorderBrush = Brush("#D7E5EA"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(16),
            Child = Stack(
                Heading("Napredni filteri", 16),
                Text("Koristite ih kada zelite tacan broj spratova, soba ili stanara.", 12, "#52677A"),
                advancedGrid,
                Row(sort))
        };
        filter.Child = Stack(
            Heading("Pronadji zgradu", 22),
            Text("Brza pretraga radi po sifri, adresi, naselju, gradu i drzavi. Detalje suzite ispod.", 13, "#52677A"),
            searchRow,
            advancedPanel);
        panel.Children.Add(filter);

        var results = Wrap();
        panel.Children.Add(results);

        void Refresh()
        {
            results.Children.Clear();
            var buildings = BuildSearch(quickSearch.Text, field.SelectedIndex, query.Text, room.Text, tenants.Text, sort.IsChecked == true);
            if (buildings.Count == 0)
            {
                results.Children.Add(Empty("Nema zgrada za izabrane filtere."));
                return;
            }

            foreach (var building in buildings)
            {
                results.Children.Add(BuildingCard(building));
            }
        }

        apply.Click += (_, _) => Refresh();
        quickSearch.TextChanged += (_, _) => Refresh();
        sort.Checked += (_, _) => Refresh();
        sort.Unchecked += (_, _) => Refresh();
        field.SelectionChanged += (_, _) =>
        {
            query.IsEnabled = field.SelectedIndex is 1 or 2 or 3;
            room.IsEnabled = field.SelectedIndex is 4 or 6 or 7;
            tenants.IsEnabled = field.SelectedIndex is 5 or 6 or 7;
        };
        field.SelectedIndex = 0;
        Refresh();
        SetContent(panel);
    }

    private List<Building> BuildSearch(string quickSearch, int fieldIndex, string query, string roomText, string tenantText, bool sort)
    {
        var quickFiltered = _services.SharedBuildings
            .GetApprovedBuildings(sort)
            .Where(building => MatchesQuickSearch(building, quickSearch))
            .ToList();

        var hasAdvancedQuery = fieldIndex > 0 &&
            (!string.IsNullOrWhiteSpace(query) ||
             !string.IsNullOrWhiteSpace(roomText) ||
             !string.IsNullOrWhiteSpace(tenantText));

        if (!hasAdvancedQuery)
        {
            return quickFiltered;
        }

        BuildingSearchCriteria criteria;
        switch (fieldIndex)
        {
            case 2:
                criteria = new BuildingSearchCriteria { Field = BuildingSearchField.Neighborhood, Query = query };
                break;
            case 3:
                criteria = new BuildingSearchCriteria { Field = BuildingSearchField.FloorCount, Query = query };
                break;
            case 4:
                criteria = new BuildingSearchCriteria
                {
                    Field = BuildingSearchField.ApartmentCriteria,
                    ApartmentCriteria = new ApartmentSearchCriteria { Mode = ApartmentSearchMode.RoomCount, RoomCount = ToInt(roomText) }
                };
                break;
            case 5:
                criteria = new BuildingSearchCriteria
                {
                    Field = BuildingSearchField.ApartmentCriteria,
                    ApartmentCriteria = new ApartmentSearchCriteria { Mode = ApartmentSearchMode.MaxTenantCount, MaxTenantCount = ToInt(tenantText) }
                };
                break;
            case 6:
            case 7:
                criteria = new BuildingSearchCriteria
                {
                    Field = BuildingSearchField.ApartmentCriteria,
                    ApartmentCriteria = new ApartmentSearchCriteria
                    {
                        Mode = ApartmentSearchMode.Combined,
                        RoomCount = ToInt(roomText),
                        MaxTenantCount = ToInt(tenantText),
                        Operator = fieldIndex == 6 ? LogicalOperator.And : LogicalOperator.Or
                    }
                };
                break;
            default:
                criteria = new BuildingSearchCriteria { Field = BuildingSearchField.Address, Query = query };
                break;
        }

        var buildings = _services.SharedBuildings.SearchApprovedBuildings(criteria);
        var quickCodes = quickFiltered.Select(building => building.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var filtered = buildings.Where(building => quickCodes.Contains(building.Code)).ToList();
        return sort ? filtered.OrderBy(building => building.FloorCount).ToList() : filtered;
    }

    private static bool MatchesQuickSearch(Building building, string quickSearch)
    {
        if (string.IsNullOrWhiteSpace(quickSearch))
        {
            return true;
        }

        var value = $"{building.Code} {building.Address.Street} {building.Address.Number} {building.Neighborhood} {building.Location.City} {building.Location.Country}";
        return value.Contains(quickSearch.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private Border BuildingCard(Building building)
    {
        var card = Card();
        card.Width = 246;
        card.MinHeight = 172;
        card.Child = Stack(
            new Border
            {
                Background = Brush("#FF5A1F"),
                Width = 48,
                Height = 5,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12)
            },
            Heading(building.Code, 22),
            Text($"{building.Address.Street} {building.Address.Number}", 16, "#111827"),
            Text($"{building.Neighborhood} - {building.Location.City}, {building.Location.Country}", 13, "#475569"),
            Badge($"{building.FloorCount} spratova", "#DBEAFE", "#1D4ED8"));
        return card;
    }

    private void ShowTenantRequests()
    {
        SetActiveNav("Moji zahtevi");
        if (_currentUser is not Tenant tenant)
        {
            return;
        }

        var panel = Page(
            "Moji zahtevi",
            "Pratite status svakog zahteva i povucite zahtev koji jos nije obradjen.",
            "Pending zahtevi mogu da se povuku.",
            "Rejected zahtevi prikazuju razlog odbijanja.");
        var filter = Combo("Svi", "Na cekanju", "Odobreni", "Odbijeni");
        panel.Children.Add(Stack(Row(Text("Status", 13, "#64748B"), filter)));
        var list = new StackPanel();
        panel.Children.Add(list);

        void Refresh()
        {
            list.Children.Clear();
            var selected = filter.SelectedIndex switch
            {
                1 => TenantRequestFilter.Pending,
                2 => TenantRequestFilter.Approved,
                3 => TenantRequestFilter.Rejected,
                _ => TenantRequestFilter.All
            };
            var requests = _services.Tenants.GetTenantRequests(tenant.Jmbg, selected);
            if (requests.Count == 0)
            {
                list.Children.Add(Empty("Nema zahteva za izabrani filter."));
                return;
            }

            foreach (var request in requests)
            {
                var card = RequestCard(request);
                if (request.Status == RequestStatus.PendingApproval)
                {
                    var withdraw = Button("Povuci zahtev", "DangerButton");
                    withdraw.Click += (_, _) =>
                    {
                        if (Confirm("Povuci zahtev koji je na cekanju?"))
                        {
                            Try(() =>
                            {
                                _services.Tenants.WithdrawRequest(tenant.Jmbg, request.Id);
                                Refresh();
                            });
                        }
                    };
                    ((StackPanel)card.Child).Children.Add(withdraw);
                }
                list.Children.Add(card);
            }
        }

        filter.SelectionChanged += (_, _) => Refresh();
        filter.SelectedIndex = 0;
        Refresh();
        SetContent(panel);
    }

    private void ShowTenantNewRequest()
    {
        SetActiveNav("Podnesi zahtev");
        if (_currentUser is not Tenant tenant)
        {
            return;
        }

        var panel = Page(
            "Novi zahtev",
            "Unesite zgradu i stan, proverite zauzetost, pa potvrdite ili promenite unos.",
            "Upozorenje za zauzet stan ne blokira slanje zahteva.",
            "Dugme za potvrdu postaje aktivno tek nakon provere.");
        var card = FormCard();
        var form = new StackPanel();
        var buildingCode = Input("Sifra zgrade");
        var apartmentNumber = Input("Broj stana");
        var state = Message();
        var check = Button("Proveri stan", "PrimaryButton");
        var create = Button("Potvrdi kreiranje zahteva", "PrimaryButton");
        var change = Button("Promeni broj stana", "SecondaryButton");
        var cancel = Button("Odustani", "SecondaryButton");
        check.IsDefault = true;
        cancel.IsCancel = true;
        create.IsEnabled = false;
        change.IsEnabled = false;

        check.Click += (_, _) =>
        {
            state.Text = "";
            if (!RequireFormFields(state, ("Sifra zgrade", buildingCode)) ||
                !TryReadPositiveInt(state, "Broj stana", apartmentNumber, out var apartmentNo))
            {
                create.IsEnabled = false;
                change.IsEnabled = false;
                return;
            }

            Try(() =>
            {
                var count = _services.Tenants.GetActiveTenantCount(buildingCode.Text.Trim(), apartmentNo);
                state.Foreground = Brush(count > 0 ? "#B45309" : "#15803D");
                state.Text = count > 0
                    ? $"Upozorenje: stan vec ima {count} aktivnih stanara."
                    : "Stan nema aktivnih stanara. Mozete poslati zahtev.";
                create.IsEnabled = true;
                change.IsEnabled = true;
            }, state);
        };
        change.Click += (_, _) =>
        {
            apartmentNumber.Focus();
            create.IsEnabled = false;
            state.Text = "Promenite broj stana i ponovo proverite.";
        };
        cancel.Click += (_, _) =>
        {
            Clear(buildingCode, apartmentNumber);
            create.IsEnabled = false;
            change.IsEnabled = false;
            state.Text = "Kreiranje zahteva je otkazano.";
        };
        create.Click += (_, _) => Try(() =>
        {
            if (!RequireFormFields(state, ("Sifra zgrade", buildingCode)) ||
                !TryReadPositiveInt(state, "Broj stana", apartmentNumber, out var apartmentNo))
            {
                create.IsEnabled = false;
                change.IsEnabled = false;
                return;
            }

            _services.Tenants.CreateAccessRequest(tenant.Jmbg, buildingCode.Text.Trim(), apartmentNo);
            state.Foreground = Brush("#15803D");
            state.Text = "Zahtev je kreiran.";
            Clear(buildingCode, apartmentNumber);
            create.IsEnabled = false;
            change.IsEnabled = false;
        }, state);

        AddAll(form,
            FormGrid(Field("Sifra zgrade", buildingCode), Field("Broj stana", apartmentNumber)),
            state,
            Row(check, create, change, cancel));
        card.Child = form;
        panel.Children.Add(card);
        SetContent(panel);
    }

    private void ShowManagerBuildings()
    {
        SetActiveNav("Moje zgrade");
        if (_currentUser is not BuildingManager manager)
        {
            return;
        }

        var panel = Page(
            "Moje zgrade",
            "Odobrite zgrade koje vam je administrator dodelio ili odbijte pogresnu dodelu.",
            "Samo odobrene zgrade postaju vidljive stanarima.",
            "Odbijena zgrada ostaje oznacena razlogom ako ga unesete.");
        var filter = Combo("Na cekanju", "Prihvacene");
        panel.Children.Add(filter);
        var actionMessage = Message();
        panel.Children.Add(actionMessage);
        var list = new StackPanel();
        panel.Children.Add(list);

        void Refresh()
        {
            list.Children.Clear();
            var selected = filter.SelectedIndex == 0 ? ManagerBuildingFilter.Pending : ManagerBuildingFilter.Approved;
            var buildings = _services.Managers.GetManagerBuildings(manager.Jmbg, selected);
            if (buildings.Count == 0)
            {
                list.Children.Add(Empty(selected == ManagerBuildingFilter.Pending ? "Nema zgrada na cekanju." : "Nema odobrenih zgrada."));
                return;
            }

            foreach (var building in buildings)
            {
                var card = BuildingCard(building);
                ((StackPanel)card.Child).Children.Add(StatusBadge(building.Status));
                if (building.Status == BuildingStatus.PendingApproval)
                {
                    var reason = Input("Razlog odbijanja (opciono)");
                    var approve = Button("Potvrdi zgradu", "PrimaryButton");
                    approve.Click += (_, _) => Try(() =>
                    {
                        _services.Managers.ApproveBuilding(manager.Jmbg, building.Code);
                        SetInlineMessage(actionMessage, "Zgrada je potvrdjena.");
                        Refresh();
                    }, actionMessage);
                    var reject = Button("Odbij zgradu", "DangerButton");
                    reject.Click += (_, _) =>
                    {
                        if (Confirm("Odbiti izabranu zgradu?"))
                        {
                            Try(() =>
                            {
                                _services.Managers.RejectBuilding(manager.Jmbg, building.Code, reason.Text);
                                SetInlineMessage(actionMessage, "Zgrada je odbijena.");
                                Refresh();
                            }, actionMessage);
                        }
                    };
                    AddAll((StackPanel)card.Child, reason, Row(approve, reject));
                }
                list.Children.Add(card);
            }
        }

        filter.SelectionChanged += (_, _) => Refresh();
        filter.SelectedIndex = 0;
        Refresh();
        SetContent(panel);
    }

    private void ShowManagerRequests()
    {
        SetActiveNav("Zahtevi");
        if (_currentUser is not BuildingManager manager)
        {
            return;
        }

        var panel = Page(
            "Zahtevi za pristup",
            "Izaberite jednu odobrenu zgradu, zatim obradite njene zahteve.",
            "Pending zahteve mozete potvrditi ili odbiti.",
            "Odbijanje zahteva zahteva obavezno obrazlozenje.");
        var buildings = _services.Managers.GetManagerBuildings(manager.Jmbg, ManagerBuildingFilter.Approved);
        var buildingCombo = Combo(buildings.Select(BuildingLabel).ToArray());
        var status = Combo("Na cekanju", "Odobreni");
        var actionMessage = Message();
        var list = new StackPanel();
        panel.Children.Add(CardWithContent(
            Heading("Izbor zgrade i statusa", 18),
            FormGrid(Field("Zgrada", buildingCombo), Field("Status zahteva", status))));
        panel.Children.Add(actionMessage);
        panel.Children.Add(list);

        void Refresh()
        {
            list.Children.Clear();
            if (buildings.Count == 0)
            {
                list.Children.Add(Empty("Nemate odobrenih zgrada."));
                return;
            }

            var building = buildings[Math.Max(0, buildingCombo.SelectedIndex)];
            var selectedStatus = status.SelectedIndex == 0 ? ManagerRequestFilter.Pending : ManagerRequestFilter.Approved;
            var requests = _services.Managers.GetManagerRequests(manager.Jmbg, building.Code, selectedStatus);
            if (requests.Count == 0)
            {
                list.Children.Add(Empty(selectedStatus == ManagerRequestFilter.Pending
                    ? "No pending requests for this building."
                    : "Nema odobrenih zahteva za ovu zgradu."));
                return;
            }

            foreach (var request in requests)
            {
                var card = RequestCard(request);
                if (request.Status == RequestStatus.PendingApproval)
                {
                    var reason = Input("Obavezno obrazlozenje za odbijanje");
                    var approve = Button("Potvrdi zahtev", "PrimaryButton");
                    approve.Click += (_, _) => Try(() =>
                    {
                        _services.Managers.ApproveRequest(manager.Jmbg, request.Id);
                        SetInlineMessage(actionMessage, "Zahtev je potvrdjen.");
                        Refresh();
                    }, actionMessage);
                    var reject = Button("Odbij zahtev", "DangerButton");
                    reject.Click += (_, _) =>
                    {
                        if (string.IsNullOrWhiteSpace(reason.Text))
                        {
                            SetInlineMessage(actionMessage, "Obrazlozenje odbijanja je obavezno.", true);
                            reason.Focus();
                            return;
                        }

                        if (Confirm("Odbiti zahtev uz uneto obrazlozenje?"))
                        {
                            Try(() =>
                            {
                                _services.Managers.RejectRequest(manager.Jmbg, request.Id, reason.Text);
                                SetInlineMessage(actionMessage, "Zahtev je odbijen.");
                                Refresh();
                            }, actionMessage);
                        }
                    };
                    AddAll((StackPanel)card.Child, reason, Row(approve, reject));
                }
                list.Children.Add(card);
            }
        }

        buildingCombo.SelectionChanged += (_, _) => Refresh();
        status.SelectionChanged += (_, _) => Refresh();
        buildingCombo.SelectedIndex = buildings.Count > 0 ? 0 : -1;
        status.SelectedIndex = 0;
        Refresh();
        SetContent(panel);
    }

    private void ShowAddApartment()
    {
        SetActiveNav("Dodaj stan");
        if (_currentUser is not BuildingManager manager)
        {
            return;
        }

        var panel = Page(
            "Unos stana",
            "Dodajte stan u zgradu kojom upravljate.",
            "Sifra zgrade mora pripadati vasoj odobrenoj zgradi.",
            "Broj stana mora biti jedinstven u okviru zgrade.");
        var card = FormCard();
        var form = new StackPanel();
        var code = Input("Sifra zgrade");
        var number = Input("Broj stana");
        var description = Input("Opis");
        var rooms = Input("Broj soba");
        var maxTenants = Input("Max broj stanara");
        _message = Message();
        var save = Button("Dodaj stan", "PrimaryButton");
        save.IsDefault = true;
        save.Click += (_, _) =>
        {
            SetMessage("");
            if (!RequireFormFields(_message, ("Sifra zgrade", code), ("Opis", description)) ||
                !TryReadPositiveInt(_message, "Broj stana", number, out var apartmentNo) ||
                !TryReadPositiveInt(_message, "Broj soba", rooms, out var roomCount) ||
                !TryReadPositiveInt(_message, "Max broj stanara", maxTenants, out var maxTenantCount))
            {
                return;
            }

            Try(() =>
            {
                _services.Managers.AddApartment(manager.Jmbg, new Apartment
                {
                    BuildingCode = code.Text.Trim(),
                    ApartmentNumber = apartmentNo,
                    Description = description.Text.Trim(),
                    RoomCount = roomCount,
                    MaxTenantCount = maxTenantCount
                });
                SetMessage("Stan je dodat.");
                Clear(code, number, description, rooms, maxTenants);
            });
        };
        var clearForm = Button("Ocisti formu", "SecondaryButton");
        clearForm.Click += (_, _) =>
        {
            Clear(code, number, description, rooms, maxTenants);
            SetMessage("");
        };
        var cancel = Button("Odustani", "SecondaryButton");
        cancel.IsCancel = true;
        cancel.Click += (_, _) => ShowDashboard();
        AddAll(form,
            FormGrid(
                Field("Sifra zgrade", code),
                Field("Broj stana", number),
                Field("Opis", description),
                Field("Broj soba", rooms),
                Field("Max broj stanara", maxTenants)),
            _message,
            Row(save, clearForm, cancel));
        card.Child = form;
        panel.Children.Add(card);
        SetContent(panel);
    }

    private void ShowAdminAddManager()
    {
        SetActiveNav("Dodaj upravnika");
        var panel = Page(
            "Dodavanje upravnika",
            "Kreirajte nalog upravnika koji kasnije moze da odobrava zgrade i zahteve.",
            "JMBG i email moraju biti jedinstveni.",
            "Nakon uspesnog dodavanja upravnik moze odmah da se prijavi.");
        var card = FormCard();
        var form = new StackPanel();
        var jmbg = Input("JMBG");
        var email = Input("Email");
        var password = Password("Lozinka");
        var first = Input("Ime");
        var last = Input("Prezime");
        var phone = Input("Mobilni telefon");
        _message = Message();
        var save = Button("Dodaj upravnika", "PrimaryButton");
        save.IsDefault = true;
        save.Click += (_, _) =>
        {
            SetMessage("");
            if (!RequireFormFields(_message, ("JMBG", jmbg), ("Email", email), ("Ime", first), ("Prezime", last), ("Mobilni telefon", phone)) ||
                !RequirePassword(_message, "Lozinka", password))
            {
                return;
            }

            Try(() =>
            {
                _services.Admins.RegisterManager(new BuildingManager
                {
                    Jmbg = jmbg.Text.Trim(),
                    Email = email.Text.Trim(),
                    Password = password.Password,
                    FirstName = first.Text.Trim(),
                    LastName = last.Text.Trim(),
                    MobilePhone = phone.Text.Trim()
                });
                SetMessage("Upravnik je dodat.");
                Clear(jmbg, email, first, last, phone);
                password.Clear();
            });
        };
        var clearForm = Button("Ocisti formu", "SecondaryButton");
        clearForm.Click += (_, _) =>
        {
            Clear(jmbg, email, first, last, phone);
            password.Clear();
            SetMessage("");
        };
        var cancel = Button("Odustani", "SecondaryButton");
        cancel.IsCancel = true;
        cancel.Click += (_, _) => ShowDashboard();
        AddAll(form,
            FormGrid(
                Field("JMBG", jmbg),
                Field("Email", email),
                Field("Lozinka", password),
                Field("Ime", first),
                Field("Prezime", last),
                Field("Mobilni telefon", phone)),
            _message,
            Row(save, clearForm, cancel));
        card.Child = form;
        panel.Children.Add(card);
        SetContent(panel);
    }

    private void ShowAdminAddBuilding()
    {
        SetActiveNav("Dodaj zgradu");
        var panel = Page(
            "Dodavanje zgrade",
            "Unesite podatke zgrade i dodelite je postojecem upravniku.",
            "Zgrada nije vidljiva stanarima dok je upravnik ne potvrdi.",
            "Sifra zgrade mora biti jedinstvena.");
        var card = FormCard();
        var form = new StackPanel();
        var code = Input("Sifra zgrade");
        var street = Input("Ulica");
        var number = Input("Broj");
        var neighborhood = Input("Naselje");
        var city = Input("Grad");
        var country = Input("Drzava");
        var floors = Input("Broj spratova");
        var managers = _services.Users.GetAll().OfType<BuildingManager>().ToList();
        var managerCombo = Combo(managers.Select(ManagerLabel).ToArray());
        _message = Message();
        var save = Button("Dodaj zgradu", "PrimaryButton");
        save.IsDefault = true;
        save.IsEnabled = managers.Count > 0;
        if (managers.Count == 0)
        {
            _message.Foreground = Brush("#B91C1C");
            _message.Text = "Prvo dodajte bar jednog upravnika.";
        }

        save.Click += (_, _) =>
        {
            SetMessage("");
            if (!RequireFormFields(_message, ("Sifra zgrade", code), ("Ulica", street), ("Broj", number), ("Naselje", neighborhood), ("Grad", city), ("Drzava", country)) ||
                !TryReadPositiveInt(_message, "Broj spratova", floors, out var floorCount))
            {
                return;
            }

            if (managerCombo.SelectedIndex < 0)
            {
                SetMessage("Izaberite postojeceg upravnika.", true);
                return;
            }

            Try(() =>
            {
                var selectedManager = managers[managerCombo.SelectedIndex];
                _services.Admins.CreateBuilding(new Building
                {
                    Code = code.Text.Trim(),
                    Address = new Address { Street = street.Text.Trim(), Number = number.Text.Trim() },
                    Neighborhood = neighborhood.Text.Trim(),
                    Location = new Location { City = city.Text.Trim(), Country = country.Text.Trim() },
                    FloorCount = floorCount,
                    ManagerJmbg = selectedManager.Jmbg
                });
                SetMessage("Zgrada je dodata i ceka odobrenje upravnika.");
                Clear(code, street, number, neighborhood, city, country, floors);
            });
        };
        var clearForm = Button("Ocisti formu", "SecondaryButton");
        clearForm.Click += (_, _) =>
        {
            Clear(code, street, number, neighborhood, city, country, floors);
            SetMessage("");
        };
        var cancel = Button("Odustani", "SecondaryButton");
        cancel.IsCancel = true;
        cancel.Click += (_, _) => ShowDashboard();
        AddAll(form,
            FormGrid(
                Field("Sifra zgrade", code),
                Field("Ulica", street),
                Field("Broj", number),
                Field("Naselje", neighborhood),
                Field("Grad", city),
                Field("Drzava", country),
                Field("Broj spratova", floors),
                Field("Postojeci upravnik", managerCombo)),
            _message,
            Row(save, clearForm, cancel));
        card.Child = form;
        panel.Children.Add(card);
        SetContent(panel);
    }

    private Border RequestCard(AccessRequest request)
    {
        var card = Card();
        card.Child = Stack(
            Row(Heading(request.Id, 18), StatusBadge(request.Status)),
            Text($"Zgrada {request.BuildingCode}, stan {request.ApartmentNumber}", 15, "#111827"),
            Text($"Stanar JMBG: {request.TenantJmbg}", 13, "#64748B"),
            Text($"Kreiran: {request.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm}", 13, "#64748B"),
            string.IsNullOrWhiteSpace(request.RejectionReason) ? new TextBlock() : Text($"Razlog: {request.RejectionReason}", 13, "#991B1B"));
        return card;
    }

    private void HandleGlobalShortcuts(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
        {
            e.Handled = true;
            if (_currentUser is null)
            {
                return;
            }

            ShowBuildings();
            Dispatcher.BeginInvoke(new Action(() => _searchBox?.Focus()));
            return;
        }

        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            if (_currentUser is null)
            {
                ShowLogin();
                return;
            }

            ShowDashboard();
        }
    }

    private void Try(Action action, TextBlock? targetMessage = null)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            if (targetMessage is not null)
            {
                targetMessage.Foreground = Brush("#B91C1C");
                targetMessage.Text = exception.Message;
            }
            else
            {
                SetMessage(exception.Message, true);
            }
        }
    }

    private static bool Confirm(string message)
    {
        return MessageBox.Show(message, "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    private void SetContent(UIElement element)
    {
        if (_content is not null)
        {
            _content.Content = new ScrollViewer { Content = element, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        }
    }

    private void SetMessage(string text, bool isError = false)
    {
        if (_message is null)
        {
            return;
        }

        _message.Foreground = Brush(isError ? "#B91C1C" : "#15803D");
        _message.Text = text;
    }

    private static void SetInlineMessage(TextBlock message, string text, bool isError = false)
    {
        message.Foreground = Brush(isError ? "#B91C1C" : "#15803D");
        message.Text = text;
    }

    private static StackPanel Page(string title, string helper, params string[] tips)
    {
        var panel = new StackPanel();
        panel.Children.Add(HeaderPanel(title, helper, tips));
        return panel;
    }

    private static Grid CenteredPage(string title, string helper)
    {
        var grid = new Grid { Margin = new Thickness(48) };
        var card = Card();
        card.Width = 520;
        card.HorizontalAlignment = HorizontalAlignment.Center;
        card.VerticalAlignment = VerticalAlignment.Center;
        card.Child = Stack(Heading(title, 28), Text(helper, 13, "#64748B"));
        grid.Children.Add(card);
        return grid;
    }

    private static Border HeroBlock(string title, string subtitle, string helper)
    {
        var border = new Border
        {
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(28),
            Margin = new Thickness(0, 0, 0, 18),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops =
                {
                    new GradientStop(Color.FromRgb(15, 118, 110), 0),
                    new GradientStop(Color.FromRgb(18, 60, 105), 1)
                }
            }
        };

        border.Child = Stack(
            new TextBlock
            {
                Text = title,
                FontSize = 54,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 6)
            },
            new TextBlock
            {
                Text = subtitle,
                FontSize = 23,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brush("#E0F2FE"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            },
            new TextBlock
            {
                Text = helper,
                FontSize = 15,
                Foreground = Brush("#D9F99D"),
                TextWrapping = TextWrapping.Wrap
            });

        return border;
    }

    private static Border HeaderPanel(string title, string helper, params string[] tips)
    {
        var content = Stack(
            Heading(title, 32),
            Text(helper, 15, "#38546D"));

        if (tips.Length > 0)
        {
            var wrap = new WrapPanel { Margin = new Thickness(0, 4, 0, 0) };
            foreach (var tip in tips)
            {
                wrap.Children.Add(Badge(tip, "#E0F2FE", "#075985"));
            }
            content.Children.Add(wrap);
        }

        return new Border
        {
            Background = Brush("#F8FBFC"),
            BorderBrush = Brush("#D7E5EA"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 18),
            Child = content
        };
    }

    private static Border InfoStrip(string title, string text)
    {
        return new Border
        {
            Background = Brush("#FFF7ED"),
            BorderBrush = Brush("#FDBA74"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(16),
            Child = Stack(
                Text(title, 13, "#9A3412"),
                Text(text, 14, "#431407"))
        };
    }

    private static Border CardWithContent(params UIElement[] elements)
    {
        var card = Card();
        card.Child = Stack(elements);
        return card;
    }

    private static Border Card()
    {
        return new Border
        {
            Background = Brush("#FFFFFF"),
            BorderBrush = Brush("#E2E8F0"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 16, 16)
        };
    }

    private static Border FormCard()
    {
        var card = Card();
        card.MaxWidth = 960;
        card.MinWidth = 760;
        card.HorizontalAlignment = HorizontalAlignment.Left;
        card.Padding = new Thickness(26);
        return card;
    }

    private static Border ActionCard(string title, string helper, string accent, Action action)
    {
        var button = Button("Otvori", "PrimaryButton");
        button.Click += (_, args) =>
        {
            args.Handled = true;
            action();
        };
        var card = Card();
        card.Width = 292;
        card.MinHeight = 198;
        card.Cursor = System.Windows.Input.Cursors.Hand;
        card.MouseLeftButtonUp += (_, _) => action();
        card.Child = Stack(
            new Border
            {
                Background = Brush(accent),
                Width = 54,
                Height = 6,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 14)
            },
            Heading(title, 22),
            Text(helper, 14, "#52677A"),
            button);
        return card;
    }

    private Button NavButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            Background = Brush("#123C69"),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(18, 14, 18, 14),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        _navButtons[text] = button;
        button.Click += (_, _) =>
        {
            SetActiveNav(text);
            action();
        };
        return button;
    }

    private void SetActiveNav(string text)
    {
        foreach (var item in _navButtons)
        {
            item.Value.Background = Brush(item.Key == text ? "#007CC2" : "#123C69");
            item.Value.FontWeight = item.Key == text ? FontWeights.SemiBold : FontWeights.Normal;
        }
    }

    private static Button Button(string text, string styleKey)
    {
        var button = new Button { Content = text };
        button.SetResourceReference(StyleProperty, styleKey);
        return button;
    }

    private static TextBox Input(string placeholder)
    {
        var box = new TextBox { Tag = placeholder };
        box.SetResourceReference(StyleProperty, "InputBox");
        box.ToolTip = placeholder;
        return box;
    }

    private static PasswordBox Password(string placeholder)
    {
        var box = new PasswordBox { Tag = placeholder, ToolTip = placeholder };
        box.SetResourceReference(StyleProperty, "PasswordInput");
        return box;
    }

    private static ComboBox Combo(params string[] values)
    {
        var combo = new ComboBox { ItemsSource = values.ToList(), SelectedIndex = values.Length > 0 ? 0 : -1 };
        combo.SetResourceReference(StyleProperty, "ComboInput");
        return combo;
    }

    private static StackPanel Field(string label, UIElement input)
    {
        if (input is TextBox textBox && string.Equals(textBox.Tag?.ToString(), label, StringComparison.OrdinalIgnoreCase))
        {
            textBox.Tag = string.Empty;
        }

        return new StackPanel
        {
            Margin = new Thickness(0, 0, 14, 8),
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brush("#38546D"),
                    Margin = new Thickness(0, 0, 0, 2)
                },
                input
            }
        };
    }

    private static UniformGrid FormGrid(params UIElement[] fields)
    {
        var grid = new UniformGrid
        {
            Columns = fields.Length <= 2 ? 2 : 2,
            Margin = new Thickness(0, 8, 0, 8)
        };
        AddAll(grid, fields);
        return grid;
    }

    private static TextBlock Heading(string text, double size)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = size,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brush("#0F172A"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
    }

    private static TextBlock Text(string text, double size, string color)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = size,
            Foreground = Brush(color),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
    }

    private static TextBlock Message()
    {
        return Text("", 13, "#15803D");
    }

    private static TextBlock Empty(string text)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = Brush("#64748B"),
            FontStyle = FontStyles.Italic,
            Margin = new Thickness(8, 18, 8, 18)
        };
    }

    private static Border Badge(string text, string background, string foreground)
    {
        return new Border
        {
            Background = Brush(background),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10, 4, 10, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new TextBlock { Text = text, Foreground = Brush(foreground), FontWeight = FontWeights.SemiBold }
        };
    }

    private static Border StatusBadge(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Approved => Badge("Approved", "#DCFCE7", "#166534"),
            RequestStatus.Rejected => Badge("Rejected", "#FEE2E2", "#991B1B"),
            _ => Badge("Pending", "#FEF3C7", "#92400E")
        };
    }

    private static Border StatusBadge(BuildingStatus status)
    {
        return status switch
        {
            BuildingStatus.Approved => Badge("Approved", "#DCFCE7", "#166534"),
            BuildingStatus.Rejected => Badge("Rejected", "#FEE2E2", "#991B1B"),
            _ => Badge("Pending", "#FEF3C7", "#92400E")
        };
    }

    private static StackPanel Stack(params UIElement[] elements)
    {
        var stack = new StackPanel();
        AddAll(stack, elements);
        return stack;
    }

    private static StackPanel Row(params UIElement[] elements)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        AddAll(row, elements);
        return row;
    }

    private static WrapPanel Wrap()
    {
        return new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
    }

    private static void AddAll(Panel panel, params UIElement[] elements)
    {
        foreach (var element in elements)
        {
            panel.Children.Add(element);
        }
    }

    private static void Clear(params TextBox[] boxes)
    {
        foreach (var box in boxes)
        {
            box.Clear();
        }
    }

    private static bool RequireFields(params TextBox[] boxes)
    {
        return boxes.All(box => !string.IsNullOrWhiteSpace(box.Text));
    }

    private static bool RequireFormFields(TextBlock message, params (string Label, TextBox Box)[] fields)
    {
        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.Box.Text))
            {
                SetInlineMessage(message, $"Polje '{field.Label}' je obavezno.", true);
                field.Box.Focus();
                return false;
            }
        }

        return true;
    }

    private static bool RequirePassword(TextBlock message, string label, PasswordBox box)
    {
        if (!string.IsNullOrWhiteSpace(box.Password))
        {
            return true;
        }

        SetInlineMessage(message, $"Polje '{label}' je obavezno.", true);
        box.Focus();
        return false;
    }

    private static bool TryReadPositiveInt(TextBlock message, string label, TextBox box, out int value)
    {
        if (int.TryParse(box.Text, out value) && value > 0)
        {
            return true;
        }

        SetInlineMessage(message, $"Polje '{label}' mora biti pozitivan ceo broj.", true);
        box.Focus();
        return false;
    }

    private static int ToInt(string text)
    {
        return int.TryParse(text, out var value) ? value : 0;
    }

    private static string RoleName(User user)
    {
        return user switch
        {
            Administrator => "Administrator",
            BuildingManager => "Upravnik",
            Tenant => "Stanar",
            _ => "Korisnik"
        };
    }

    private static string BuildingLabel(Building building)
    {
        return $"{building.Code} - {building.Address.Street} {building.Address.Number}";
    }

    private static string ManagerLabel(BuildingManager manager)
    {
        return $"{manager.FirstName} {manager.LastName} ({manager.Jmbg})";
    }

    private static SolidColorBrush Brush(string color)
    {
        return (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
    }
}
