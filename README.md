# SIMS Projekat

GitHub repozitorijum: https://github.com/Jaddes/SIMS-projekat

Konzolna C# aplikacija za upravljanje zgradama, stanovima, korisnicima i zahtevima stanara za pristup zgradi/stanu. Projekat je uradjen prema specifikaciji "Specifikacija projekta za nize ocene - 2026".

## Pokretanje

Preduslov: instaliran .NET 8 SDK.

```powershell
cd D:\SIMS\Projekat\SIMSProject
dotnet run --project SIMSProject.csproj
```

Build provera:

```powershell
dotnet build SIMSProject.csproj --no-restore
```

## Test nalozi

| Uloga | Email | Lozinka | JMBG |
| --- | --- | --- | --- |
| Administrator | admin@sims.local | Admin123! | 0000000000000 |
| Upravnik | upravnik@sims.local | Upravnik123! | 1111111111111 |
| Upravnik 2 | upravnik2@sims.local | Upravnik234! | 2222222222222 |
| Stanar | stanar@sims.local | Stanar123! | 3333333333333 |
| Stanar 2 | stanar2@sims.local | Stanar234! | 4444444444444 |

Administrator je inicijalno upisan u `Storage/users.json`.

## Podaci

Svi entiteti se cuvaju u zasebnim JSON datotekama:

| Entitet | Datoteka |
| --- | --- |
| Korisnici | `Storage/users.json` |
| Zgrade | `Storage/buildings.json` |
| Stanovi | `Storage/apartments.json` |
| Zahtevi za pristup | `Storage/accessRequests.json` |
| Clanstva stanara u stanovima | `Storage/apartmentMemberships.json` |

## Funkcionalnosti

Zajednicko za sve korisnike:
- Prijava preko iste forme za administratora, upravnika i stanara.
- Prikaz svih odobrenih zgrada bez JMBG-a upravnika.
- Izbor da se prikaz odobrenih zgrada sortira po broju spratova.
- Pretraga zgrada po adresi, naselju, broju spratova i stanovima.
- Pretraga po stanovima po broju soba, max broju stanara ili kombinaciji `broj soba & max stanara` / `broj soba | max stanara`.

Stanar:
- Registracija stanara uz proveru jedinstvenog email-a i jedinstvene lozinke.
- Podnosenje zahteva za pristup odobrenoj zgradi i postojecem stanu.
- Upozorenje ako stan vec ima aktivnog stanara, uz mogucnost potvrde, izmene unosa ili odustajanja.
- Prikaz svih svojih zahteva, uz filter za sve, na cekanju, odobrene i odbijene.
- Prikaz razloga odbijanja kod odbijenih zahteva.
- Povlacenje zahteva koji su na cekanju.

Upravnik:
- Prikaz svojih zgrada na cekanju i odobrenih zgrada.
- Potvrda ili odbijanje zgrada koje je administrator uneo za tog upravnika.
- Unos stanova samo u svoje odobrene zgrade.
- Izbor jedne svoje odobrene zgrade pre pregleda ili obrade zahteva.
- Prikaz zahteva za izabranu zgradu, uz filter za zahteve na cekanju ili odobrene zahteve.
- Potvrda zahteva na cekanju, cime nastaje aktivno clanstvo stanara u stanu.
- Odbijanje zahteva na cekanju uz obavezno obrazlozenje.

Administrator:
- Registracija novih upravnika uz proveru jedinstvenog JMBG-a i email-a.
- Unos zgrada sa svim obaveznim podacima i JMBG-om postojeceg upravnika.
- Uneta zgrada ima status na cekanju dok je dodeljeni upravnik ne potvrdi.

## UML

UML dijagrami su u `Docs/UML`:
- `UseCaseDiagram.puml`
- `ClassDiagram.puml`

Klasni dijagram prikazuje Model, Service i Repository sloj i uskladjen je sa javnim klasama, interfejsima, metodama, svojstvima, enum vrednostima i zavisnostima u kodu.

## Checklist zahteva

- [x] Use Case dijagram postoji.
- [x] Class dijagram za Model, Service i Repository sloj postoji.
- [x] Kod je implementiran u C# konzolnoj aplikaciji.
- [x] Kod se poklapa sa klasnim dijagramom.
- [x] Obavezni atributi modela nisu uklonjeni niti izmenjeni.
- [x] Clean Code principi su ispostovani kroz odvojene modele, repozitorijume, servise, menije i validacije.
- [x] Svi entiteti se cuvaju u zasebnim datotekama.
- [x] Administrator je inicijalno upisan u datoteku.
- [x] Login je zajednicki za sve tipove korisnika.
- [x] Stanar moze da se registruje uz jedinstven email i jedinstvenu lozinku.
- [x] Administrator dodaje upravnike.
- [x] Administrator dodaje zgrade.
- [x] Upravnik potvrdjuje ili odbija zgrade.
- [x] Upravnik dodaje stanove u svoje odobrene zgrade.
- [x] Stanar salje zahtev za pristup zgradi/stanu.
- [x] Stanar vidi, filtrira i povlaci svoje zahteve.
- [x] Upravnik vidi, filtrira, potvrdjuje i odbija zahteve za izabranu svoju zgradu.
- [x] Odbijanje zahteva zahteva obavezno obrazlozenje.
- [x] Odobrene zgrade se prikazuju bez JMBG-a upravnika.
- [x] Prikaz zgrada nudi sortiranje po broju spratova.
- [x] Pretraga zgrada podrzava adresu, naselje, broj spratova i stanove.
- [x] Pretraga stanova podrzava broj soba, max broj stanara i kombinacije sa `&` i `|`.
