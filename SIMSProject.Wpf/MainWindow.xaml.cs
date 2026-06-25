using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SIMSProject.Enums;
using SIMSProject.Models;

namespace SIMSProject.Wpf;

public partial class MainWindow : Window
{
    private readonly AppServices _services = new();
    private User? _currentUser;
    private ContentControl? _content;
    private TextBlock? _message;
    private TextBlock? _userLabel;

    public MainWindow()
    {
        InitializeComponent();
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
        intro.Children.Add(Heading("SIMS Buildings", 42));
        intro.Children.Add(Text("Pregledajte zgrade, upravljajte stanovima i obradjujte zahteve kroz moderan desktop interfejs.", 17, "#475569"));
        intro.Children.Add(Text("Sve funkcionalnosti iz specifikacije dostupne su kroz GUI.", 14, "#64748B"));
        Grid.SetColumn(intro, 0);
        page.Children.Add(intro);

        var loginCard = Card();
        loginCard.Width = 390;
        loginCard.VerticalAlignment = VerticalAlignment.Center;
        loginCard.HorizontalAlignment = HorizontalAlignment.Center;
        var form = new StackPanel();
        form.Children.Add(Heading("Prijava", 28));
        form.Children.Add(Text("Jedna forma za sve korisnike.", 13, "#64748B"));
        var email = Input("Email");
        var password = Password("Lozinka");
        _message = Message();
        form.Children.Add(email);
        form.Children.Add(password);
        form.Children.Add(_message);

        var loginButton = Button("Login", "PrimaryButton");
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
        var page = CenteredPage("Registracija stanara", "Email i lozinka moraju biti jedinstveni po specifikaciji.");
        var form = (StackPanel)((Border)page.Children[0]).Child;
        var jmbg = Input("JMBG");
        var email = Input("Email");
        var password = Password("Lozinka");
        var firstName = Input("Ime");
        var lastName = Input("Prezime");
        var phone = Input("Mobilni telefon");
        _message = Message();
        AddAll(form, jmbg, email, password, firstName, lastName, phone, _message);

        var actions = Row();
        var save = Button("Registruj se", "PrimaryButton");
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
        back.Click += (_, _) => ShowLogin();
        actions.Children.Add(save);
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
        var shell = new Grid();
        shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.RowDefinitions.Add(new RowDefinition());
        shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
        shell.ColumnDefinitions.Add(new ColumnDefinition());

        var top = new DockPanel { Background = Brush("#FFFFFF"), LastChildFill = false, Margin = new Thickness(0, 0, 0, 1) };
        top.Children.Add(Heading("SIMS Buildings", 22));
        _userLabel = Text($"{_currentUser.FirstName} {_currentUser.LastName} - {RoleName(_currentUser)}", 14, "#334155");
        _userLabel.Margin = new Thickness(0, 12, 16, 0);
        DockPanel.SetDock(_userLabel, Dock.Right);
        top.Children.Add(_userLabel);
        var logout = Button("Logout", "SecondaryButton");
        logout.Click += (_, _) => ShowLogin();
        DockPanel.SetDock(logout, Dock.Right);
        top.Children.Add(logout);
        Grid.SetColumnSpan(top, 2);
        shell.Children.Add(top);

        var nav = new StackPanel { Background = Brush("#0F172A"), Margin = new Thickness(0), VerticalAlignment = VerticalAlignment.Stretch };
        nav.Children.Add(NavButton("Dashboard", ShowDashboard));
        nav.Children.Add(NavButton("Zgrade", ShowBuildings));
        if (_currentUser is Tenant)
        {
            nav.Children.Add(NavButton("Moji zahtevi", ShowTenantRequests));
            nav.Children.Add(NavButton("Novi zahtev", ShowTenantNewRequest));
        }
        else if (_currentUser is BuildingManager)
        {
            nav.Children.Add(NavButton("Moje zgrade", ShowManagerBuildings));
            nav.Children.Add(NavButton("Zahtevi", ShowManagerRequests));
            nav.Children.Add(NavButton("Unos stana", ShowAddApartment));
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
        var panel = Page("Dashboard", "Sve dostupne akcije su vidljive za trenutnu ulogu.");
        var cards = Wrap();
        cards.Children.Add(ActionCard("Zgrade", "Pretraga, filteri, sortiranje i pregled kartica.", ShowBuildings));
        if (_currentUser is Tenant)
        {
            cards.Children.Add(ActionCard("Moji zahtevi", "Pregled, filter i povlacenje zahteva.", ShowTenantRequests));
            cards.Children.Add(ActionCard("Novi zahtev", "Posaljite zahtev za pristup zgradi i stanu.", ShowTenantNewRequest));
        }
        else if (_currentUser is BuildingManager)
        {
            cards.Children.Add(ActionCard("Moje zgrade", "Odobrite ili odbijte zgrade na cekanju.", ShowManagerBuildings));
            cards.Children.Add(ActionCard("Zahtevi", "Izaberite zgradu i obradite zahteve.", ShowManagerRequests));
            cards.Children.Add(ActionCard("Unos stana", "Dodajte stan u svoju odobrenu zgradu.", ShowAddApartment));
        }
        else if (_currentUser is Administrator)
        {
            cards.Children.Add(ActionCard("Dodaj upravnika", "Registrujte novog upravnika.", ShowAdminAddManager));
            cards.Children.Add(ActionCard("Dodaj zgradu", "Unesite zgradu i dodelite upravnika.", ShowAdminAddBuilding));
        }

        panel.Children.Add(cards);
        SetContent(panel);
    }

    private void ShowBuildings()
    {
        var panel = Page("Zgrade", "Odobrene zgrade su prikazane bez JMBG-a upravnika.");
        var filter = Card();
        var filterGrid = new Grid();
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition());
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });

        var field = Combo("Adresa", "Naselje", "Broj spratova", "Broj soba", "Max stanara", "Sobe & stanari", "Sobe | stanari");
        var query = Input("Vrednost pretrage");
        var room = Input("Broj soba");
        var tenants = Input("Max stanara");
        var sort = new CheckBox { Content = "Sortiraj po spratovima", Margin = new Thickness(8, 12, 8, 0) };
        var apply = Button("Primeni", "PrimaryButton");
        Grid.SetColumn(field, 0);
        Grid.SetColumn(query, 1);
        Grid.SetColumn(room, 2);
        Grid.SetColumn(tenants, 3);
        Grid.SetColumn(sort, 4);
        AddAll(filterGrid, field, query, room, tenants, sort);
        filter.Child = Stack(filterGrid, apply);
        panel.Children.Add(filter);

        var results = Wrap();
        panel.Children.Add(results);

        void Refresh()
        {
            results.Children.Clear();
            var buildings = BuildSearch(field.SelectedIndex, query.Text, room.Text, tenants.Text, sort.IsChecked == true);
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
        field.SelectionChanged += (_, _) =>
        {
            query.IsEnabled = field.SelectedIndex <= 2;
            room.IsEnabled = field.SelectedIndex is 3 or 5 or 6;
            tenants.IsEnabled = field.SelectedIndex is 4 or 5 or 6;
        };
        field.SelectedIndex = 0;
        Refresh();
        SetContent(panel);
    }

    private List<Building> BuildSearch(int fieldIndex, string query, string roomText, string tenantText, bool sort)
    {
        var hasQuery = !string.IsNullOrWhiteSpace(query) || !string.IsNullOrWhiteSpace(roomText) || !string.IsNullOrWhiteSpace(tenantText);
        if (!hasQuery)
        {
            return _services.SharedBuildings.GetApprovedBuildings(sort);
        }

        BuildingSearchCriteria criteria;
        switch (fieldIndex)
        {
            case 1:
                criteria = new BuildingSearchCriteria { Field = BuildingSearchField.Neighborhood, Query = query };
                break;
            case 2:
                criteria = new BuildingSearchCriteria { Field = BuildingSearchField.FloorCount, Query = query };
                break;
            case 3:
                criteria = new BuildingSearchCriteria
                {
                    Field = BuildingSearchField.ApartmentCriteria,
                    ApartmentCriteria = new ApartmentSearchCriteria { Mode = ApartmentSearchMode.RoomCount, RoomCount = ToInt(roomText) }
                };
                break;
            case 4:
                criteria = new BuildingSearchCriteria
                {
                    Field = BuildingSearchField.ApartmentCriteria,
                    ApartmentCriteria = new ApartmentSearchCriteria { Mode = ApartmentSearchMode.MaxTenantCount, MaxTenantCount = ToInt(tenantText) }
                };
                break;
            case 5:
            case 6:
                criteria = new BuildingSearchCriteria
                {
                    Field = BuildingSearchField.ApartmentCriteria,
                    ApartmentCriteria = new ApartmentSearchCriteria
                    {
                        Mode = ApartmentSearchMode.Combined,
                        RoomCount = ToInt(roomText),
                        MaxTenantCount = ToInt(tenantText),
                        Operator = fieldIndex == 5 ? LogicalOperator.And : LogicalOperator.Or
                    }
                };
                break;
            default:
                criteria = new BuildingSearchCriteria { Field = BuildingSearchField.Address, Query = query };
                break;
        }

        var buildings = _services.SharedBuildings.SearchApprovedBuildings(criteria);
        return sort ? buildings.OrderBy(building => building.FloorCount).ToList() : buildings;
    }

    private Border BuildingCard(Building building)
    {
        var card = Card();
        card.Width = 310;
        card.Child = Stack(
            Heading(building.Code, 22),
            Text($"{building.Address.Street} {building.Address.Number}", 16, "#111827"),
            Text($"{building.Neighborhood} - {building.Location.City}, {building.Location.Country}", 13, "#475569"),
            Badge($"{building.FloorCount} spratova", "#DBEAFE", "#1D4ED8"));
        return card;
    }

    private void ShowTenantRequests()
    {
        if (_currentUser is not Tenant tenant)
        {
            return;
        }

        var panel = Page("Moji zahtevi", "Filtrirajte zahteve i povucite one koji su na cekanju.");
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
        if (_currentUser is not Tenant tenant)
        {
            return;
        }

        var panel = Page("Novi zahtev", "Proverite stan, zatim potvrdite zahtev ili promenite unos.");
        var card = Card();
        var form = new StackPanel();
        var buildingCode = Input("Sifra zgrade");
        var apartmentNumber = Input("Broj stana");
        var state = Message();
        var check = Button("Proveri stan", "PrimaryButton");
        var create = Button("Potvrdi kreiranje zahteva", "PrimaryButton");
        var change = Button("Promeni broj stana", "SecondaryButton");
        var cancel = Button("Odustani", "SecondaryButton");
        create.IsEnabled = false;
        change.IsEnabled = false;

        check.Click += (_, _) =>
        {
            state.Text = "";
            Try(() =>
            {
                var count = _services.Tenants.GetActiveTenantCount(buildingCode.Text.Trim(), ToInt(apartmentNumber.Text));
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
            _services.Tenants.CreateAccessRequest(tenant.Jmbg, buildingCode.Text.Trim(), ToInt(apartmentNumber.Text));
            state.Foreground = Brush("#15803D");
            state.Text = "Zahtev je kreiran.";
            Clear(buildingCode, apartmentNumber);
            create.IsEnabled = false;
            change.IsEnabled = false;
        }, state);

        AddAll(form, buildingCode, apartmentNumber, state, Row(check, create, change, cancel));
        card.Child = form;
        panel.Children.Add(card);
        SetContent(panel);
    }

    private void ShowManagerBuildings()
    {
        if (_currentUser is not BuildingManager manager)
        {
            return;
        }

        var panel = Page("Moje zgrade", "Pregledajte zgrade na cekanju i potvrdite ili odbijte pogresne dodele.");
        var filter = Combo("Na cekanju", "Prihvacene");
        panel.Children.Add(filter);
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
                        Refresh();
                    });
                    var reject = Button("Odbij zgradu", "DangerButton");
                    reject.Click += (_, _) =>
                    {
                        if (Confirm("Odbiti izabranu zgradu?"))
                        {
                            Try(() =>
                            {
                                _services.Managers.RejectBuilding(manager.Jmbg, building.Code, reason.Text);
                                Refresh();
                            });
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
        if (_currentUser is not BuildingManager manager)
        {
            return;
        }

        var panel = Page("Zahtevi za pristup", "Prvo izaberite jednu svoju odobrenu zgradu.");
        var buildings = _services.Managers.GetManagerBuildings(manager.Jmbg, ManagerBuildingFilter.Approved);
        var buildingCombo = Combo(buildings.Select(BuildingLabel).ToArray());
        var status = Combo("Na cekanju", "Odobreni");
        var list = new StackPanel();
        panel.Children.Add(Stack(Text("Zgrada", 13, "#64748B"), buildingCombo, Text("Status zahteva", 13, "#64748B"), status));
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
                        Refresh();
                    });
                    var reject = Button("Odbij zahtev", "DangerButton");
                    reject.Click += (_, _) =>
                    {
                        if (string.IsNullOrWhiteSpace(reason.Text))
                        {
                            MessageBox.Show("Obrazlozenje odbijanja je obavezno.", "Validacija", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (Confirm("Odbiti zahtev uz uneto obrazlozenje?"))
                        {
                            Try(() =>
                            {
                                _services.Managers.RejectRequest(manager.Jmbg, request.Id, reason.Text);
                                Refresh();
                            });
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
        if (_currentUser is not BuildingManager manager)
        {
            return;
        }

        var panel = Page("Unos stana", "Stan se moze dodati samo u vasu odobrenu zgradu.");
        var card = Card();
        var form = new StackPanel();
        var code = Input("Sifra zgrade");
        var number = Input("Broj stana");
        var description = Input("Opis");
        var rooms = Input("Broj soba");
        var maxTenants = Input("Max broj stanara");
        _message = Message();
        var save = Button("Dodaj stan", "PrimaryButton");
        save.Click += (_, _) => Try(() =>
        {
            _services.Managers.AddApartment(manager.Jmbg, new Apartment
            {
                BuildingCode = code.Text.Trim(),
                ApartmentNumber = ToInt(number.Text),
                Description = description.Text.Trim(),
                RoomCount = ToInt(rooms.Text),
                MaxTenantCount = ToInt(maxTenants.Text)
            });
            SetMessage("Stan je dodat.");
            Clear(code, number, description, rooms, maxTenants);
        });
        AddAll(form, code, number, description, rooms, maxTenants, _message, save);
        card.Child = form;
        panel.Children.Add(card);
        SetContent(panel);
    }

    private void ShowAdminAddManager()
    {
        var panel = Page("Dodavanje upravnika", "Sistem sprecava dupli JMBG i email.");
        var card = Card();
        var form = new StackPanel();
        var jmbg = Input("JMBG");
        var email = Input("Email");
        var password = Password("Lozinka");
        var first = Input("Ime");
        var last = Input("Prezime");
        var phone = Input("Mobilni telefon");
        _message = Message();
        var save = Button("Dodaj upravnika", "PrimaryButton");
        save.Click += (_, _) => Try(() =>
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
        AddAll(form, jmbg, email, password, first, last, phone, _message, save);
        card.Child = form;
        panel.Children.Add(card);
        SetContent(panel);
    }

    private void ShowAdminAddBuilding()
    {
        var panel = Page("Dodavanje zgrade", "Zgrada ceka potvrdu upravnika ciji je JMBG unet.");
        var card = Card();
        var form = new StackPanel();
        var code = Input("Sifra zgrade");
        var street = Input("Ulica");
        var number = Input("Broj");
        var neighborhood = Input("Naselje");
        var city = Input("Grad");
        var country = Input("Drzava");
        var floors = Input("Broj spratova");
        var managerJmbg = Input("JMBG upravnika");
        _message = Message();
        var save = Button("Dodaj zgradu", "PrimaryButton");
        save.Click += (_, _) => Try(() =>
        {
            _services.Admins.CreateBuilding(new Building
            {
                Code = code.Text.Trim(),
                Address = new Address { Street = street.Text.Trim(), Number = number.Text.Trim() },
                Neighborhood = neighborhood.Text.Trim(),
                Location = new Location { City = city.Text.Trim(), Country = country.Text.Trim() },
                FloorCount = ToInt(floors.Text),
                ManagerJmbg = managerJmbg.Text.Trim()
            });
            SetMessage("Zgrada je dodata i ceka odobrenje upravnika.");
            Clear(code, street, number, neighborhood, city, country, floors, managerJmbg);
        });
        AddAll(form, code, street, number, neighborhood, city, country, floors, managerJmbg, _message, save);
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

    private static StackPanel Page(string title, string helper)
    {
        return Stack(Heading(title, 30), Text(helper, 14, "#64748B"));
    }

    private static Grid CenteredPage(string title, string helper)
    {
        var grid = new Grid { Margin = new Thickness(48) };
        var card = Card();
        card.Width = 480;
        card.HorizontalAlignment = HorizontalAlignment.Center;
        card.VerticalAlignment = VerticalAlignment.Center;
        card.Child = Stack(Heading(title, 28), Text(helper, 13, "#64748B"));
        grid.Children.Add(card);
        return grid;
    }

    private static Border Card()
    {
        return new Border
        {
            Background = Brush("#FFFFFF"),
            BorderBrush = Brush("#E2E8F0"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 16, 16)
        };
    }

    private static Border ActionCard(string title, string helper, Action action)
    {
        var button = Button("Otvori", "PrimaryButton");
        button.Click += (_, _) => action();
        var card = Card();
        card.Width = 280;
        card.Child = Stack(Heading(title, 20), Text(helper, 13, "#64748B"), button);
        return card;
    }

    private static Button NavButton(string text, Action action)
    {
        var button = new Button
        {
            Content = text,
            Background = Brush("#0F172A"),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(18, 14, 18, 14),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        button.Click += (_, _) => action();
        return button;
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
            CornerRadius = new CornerRadius(999),
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

    private static SolidColorBrush Brush(string color)
    {
        return (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
    }
}
