# Branding EnsyLabs - Enclave

Nume produs: Enclave
Nume companie: EnsyLabs

## Paleta de culori (https://coolors.co/14140c-e8c40f-c38e11-faf7ec)

(dark mode)
Negru cald => #14140C

Gold - bright => #E8C40F (accent primar)

Gold - bronz => #C38E11 (accent secundar)

Trebuie si un toggle pt dark/light mode. Pt paleta de culori de la light mode poti sa alegi tu culoarea de background (poate #FAF7EC), dar culorile de accent ar trebui sa ramana
daca nu reusesti sa faci sa mearga paleta asta de culor sa imi zici si discutam alta

## Produse

### Licensing - admin backoffice (pt mine)

#### Pagini

1. Products - lista de produse
2. Organizations - lista cu toate "organizatiile" 
3. Organization details - detalii despre o organizatie
4. Licenses - lista de licente asignate (active/expirate/aproape expirate)
5. License details -  detalii despre o licenta
6. License Requests - lista de request-uri de la clienti pentru asignare/renew la licenta
7. License Request details - detalii despre request-ul de licenta

#### **Products**

- tabel cu toate produsele
  - name
  - status
    1. active
    2. retired
  - buton de edit product
  - buton de retire
  - buton de delete
- search bar pentru produse
- buton de adaugat produs nou (deasupra la table)

cand apesi butonul de create se deschide un form peste pagina curenta cu urmatoarele field-uri

1. Name (required)
2. Description

#### **Organizations**

- tabel cu toate organizatiile
  - name
  - status
    1. active
    2. unlicensed
    3. license near expiry
    4. deactivated
  - primary contact
    - email
- la click pe un rand => mergi la pagina de Organization details
- search bar pt organizatii
- ceva filter bar ca sa pot sa filtrez organizatii
  - dupa status
  - ma mai gandesc eu la alte criterii de filtrare
- buton de adaugat organizatie noua

cand apesi butonul de create se deschide un form peste pagina curenta

1. name
2. admin email

#### **Organization details**

- 3 sectiuni
  - info
    - detalii despre organizatie (overview)
    - buton de edit ca sa pot edita informatiile (devin field-urile din overview editabile si butonul de edit e inlocuit de 2 butoane noi: Cancel si Save)
    - buton de Deactivate/Activate (toggle) - seteaza manual statusul organizatiei la "deactivated", suprascriind orice status calculat (active/unlicensed/license near expiry) care ar aparea altfel. Daca apesi din nou, organizatia se reactiveaza si revine la statusul calculat pe baza licentelor ei.
  - Licenses
    - lista de licente pe care le are organizatia
      - nume produs
      - cand expira
      - buton de editare licenta
    - buton de grant license
  - users
    - lista de useri
      - nume
      - email
      - rol (admin, reader)
      - status (active, deactivated, invite sent)
      - buton de edit
      - buton de deactivate
      - buton de delete
    - buton de invite user
  
cand apesi grant license se deschide form (peste pagina curenta)

1. dropdown cu produsele
2. date picker pt start date cu default la "azi" (data de la care e valida licenta)
3. date picker pt end date (data pana la care e valida licenta)

cand apesi add user se deschide form (peste pagina curenta)

formul e de tip lista, adica pot invita mai multi useri deodata

1. email
2. rol

#### Licenses

- tabel cu toate licentele
  - organization
  - product
  - status
    1. active
    2. expired
    3. suspended
    4. revoked
  - expire date (poate merita sa fie pe aceasi coloana status cu data de expirare idk)
  - time left until expiry
- meniu de filtrare
  - dupa organizatie
  - dupa produs
  - dupa status
- meniu de sortare (poate pe fiecare header la coloana)

la click pe rand => se deschide License Details

#### License details

- sectiuni
  - info
    - detalii despre licenta
      - produs
      - organizatie
      - start date
      - expiry date
      - status
      - buton de edit (devine editabil doar start date si expiry date)
  - sectiune de renewal requests
    - apare doar cand sunt cereri de licenta de la client
    - in loc de sectiune separata, poate sa fie un header la pagina care sa zica "Client has requested a license renewal" cu un link spre pagina de  License Requests

butoane deasupra la sectiuni

1. Suspend
2. Revoke

#### License requests

nu exista un field separat de "type" pentru new vs. renewal - se infera pe backend: daca exista deja o licenta pentru acel org+product, e renewal; daca nu exista, e cerere de licenta noua

- lista de request-uri de licenta
  - organizatie
  - product
  - existing license (link la licenta existenta, daca exista)
  - Requested By
  - requested on
  - status (pending/approved/rejected)

cand apesi pe un rand deschizi License Request details

#### License Request details

- 2 butoane de actiune
  - approve
    - daca e o cerere de **licenta noua** (nu exista licenta) - se deschide un meniu cu 2 date pickers: start date, end date
    - daca e o cerere de **renewal** (exista deja licenta) - se deschide un meniu cu un singur date picker: new expiry date (start date-ul licentei nu se schimba, doar expirarea se muta)
  - reject
    - cand apesi se deschide un meniu cu text box: reason (optional)
- detaliile care sa apara pe pagina
  - requested by
  - existing license (link la licenta existenta, daca exista)
  - notes (detalii de la client)

### Licensing - customer backoffice (pt clienti logati)

#### Pagini

1. My Licenses - lista de licente
2. License details - detalii despre o licenta
3. My License Requests - lista de cereri de licenta
4. Users - lista de utilizatori

#### **My Licenses**

- lista cu toate licentele
  - product
  - status
  - expiry date
  - time left until expiry
  - buton de "Request Renewal" - apare doar cand licenta e expirata sau aproape expirata
- buton de "Request License'

cand apesi pe un rand => deschizi License Details
cand apesi pe Request Renewal/Request License apare form (peste ecranul curent)

1. Product (daca Request Renewal a fost apasat atunci e prepopulat cu produsul de pe care s-a apasat)
2. Notes - text box in care clientul adauga notite

#### License Details (asta poate ar fi o idee sa fie tot un pop-up ca ala de la My License Requests ca sa nu fie prea multe pagini pt clienti)

- aceleasi detalii ca si cele care apar pe pagina de My Licenses
- sectiune de license history (inspiratie istoricul de la admin backoffice pt License Details)
- buton de Request Renewal - apare tot asa ca si mai sus

#### My License Requests

- lista cu toate requesturile de licentiere
  - product
  - requested on
  - requested by
  - status
  - buton de delete (anulare cerere) - apare doar cand statusul e pending

cand se apasa pe un rand se deschide un pop-up cu detalii

1. product
2. requested on
3. requested by
4. notes (notele de la client)
5. status
6. rejection reason (apare doar daca statusl e rejected)
7. buton de delete/cancel - apare doar cand statusul e pending

#### Users

- buton de invite new user (behavior ca la Invite Users de la admin backoffice)
- lista cu toti userii din acest org
  - name
  - email
  - status
  - role
  - buton de "Edit"
  - buton de "Deactivate"
  - buton de "Delete"

la buton de edit apare un form (peste ecranul curent)

1. name - pt cand trebuie sa schimbe numele unui user
2. email - cand schimba emailul??? (poate nu are sens, idk)
3. role

### Pagini aditionale

- Not found
  - pagina care sa spuna ca nu a fost gasita aceasta pagina; se arata si in loc de o pagina dedicata "Forbidden" cand un user logat da peste ceva la care nu are acces, ca sa nu poata sa-si dea seama daca o resursa exista dar e blocata, sau pur si simplu nu exista

### Elemente aditionale

#### Header

- logo EnsyLabs?
- toggle intre dark si light mode
- current identity (detalii despre cine e logat) - poate asta e doar o iconita de user + un nume si cand se deschide se arata ce e mai jos
  - name
  - email
  - role
  - buton de logout

#### Sidebar

sidebar pt navigare de la o pagina la alta

- admin backoffice
  - Products
  - Organizations
  - Licenses
  - License Requests (la asta poate sa apara si un badge cu un nr de requesturi)
- customer backoffice
  - my licenses
  - my license requests
  - users

ideal sa fie colapsabil acest sidebar si cand se face collaps sa apara doar iconitele (+ badge-ul ala de la License Requests)

#### Error message toast

un toast care sa apara in josul paginii cand apare o eroare, are un "loading bar" pana se inchide si un buton de x, la hover loading bar-ul se pune pe pauza
