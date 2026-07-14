# Branding EnsyLabs - Enclave

Product name: Enclave
Company name: EnsyLabs

## Color palette (https://coolors.co/14140c-e8c40f-c38e11-faf7ec)

(dark mode)
Warm black => #14140C

Gold - bright => #E8C40F (primary accent)

Gold - bronze => #C38E11 (secondary accent)

There also needs to be a dark/light mode toggle. For the light mode color palette you can choose the background color yourself (maybe #FAF7EC), but the accent colors should stay the same.
If you can't get this color palette to work, let me know and we'll discuss another one.

## Products

### Licensing - admin backoffice (for me)

#### Pages

1. Products - list of products
2. Organizations - list of all "organizations"
3. Organization details - details about an organization
4. Licenses - list of assigned licenses (active/expired/near expiry)
5. License details - details about a license
6. License Requests - list of requests from clients for assigning/renewing a license
7. License Request details - details about the license request

#### **Products**

- table with all the products
  - name
  - status
    1. active
    2. retired
  - edit product button
  - retire button
  - delete button
- search bar for products
- button to add a new product (above the table)

when you click the create button, a form opens over the current page with the following fields

1. Name (required)
2. Description

#### **Organizations**

- table with all the organizations
  - name
  - status
    1. active
    2. unlicensed
    3. license near expiry
    4. deactivated
  - primary contact
    - email
- clicking on a row => goes to the Organization details page
- search bar for organizations
- some filter bar so I can filter organizations
  - by status
  - I'll think about other filtering criteria
- button to add a new organization

when you click the create button, a form opens over the current page

1. name
2. admin email

#### **Organization details**

- 3 sections
  - info
    - details about the organization (overview)
    - edit button so I can edit the information (the overview fields become editable and the edit button is replaced by 2 new buttons: Cancel and Save)
    - Deactivate/Activate button (toggle) - manually sets the org's status to "deactivated", overriding whatever the computed status (active/unlicensed/license near expiry) would otherwise show. Clicking it again re-activates the org, which then falls back to whatever its computed status is based on its licenses.
  - Licenses
    - list of licenses the organization has
      - product name
      - when it expires
      - edit license button
    - grant license button
  - users
    - list of users
      - name
      - email
      - role (admin, reader)
      - status (active, deactivated, invite sent)
      - edit button
      - deactivate button
      - delete button
    - invite user button

when you click grant license, a form opens (over the current page)

1. dropdown with the products
2. date picker for start date, defaulting to "today" (the date from which the license is valid)
3. date picker for end date (the date until which the license is valid)

when you click add user, a form opens (over the current page)

the form is list-type, meaning you can invite multiple users at once

1. email
2. role

#### Licenses

- table with all the licenses
  - organization
  - product
  - status
    1. active
    2. expired
    3. suspended
    4. revoked
  - expire date (maybe it's worth having this in the same column as status with the expiry date, idk)
  - time left until expiry
- filter menu
  - by organization
  - by product
  - by status
- sort menu (maybe on each column header)

clicking on a row => opens License Details

#### License details

- sections
  - info
    - details about the license
      - product
      - organization
      - start date
      - expiry date
      - status
      - edit button (only start date and expiry date become editable)
  - renewal requests section
    - only appears when there are license requests from the client
    - instead of a separate section, maybe it could be a page header that says "Client has requested a license renewal" with a link to the License Requests page

buttons above the sections

1. Suspend
2. Revoke

#### License requests

there's no separate "type" field for new vs. renewal - it's inferred on the backend: if a license already exists for that org+product, it's a renewal; if it doesn't, it's a new license request

- list of license requests
  - organization
  - product
  - existing license (link to the existing license, if one exists)
  - Requested By
  - requested on
  - status (pending/approved/rejected)

when you click on a row, it opens License Request details

#### License Request details

- 2 action buttons
  - approve
    - if this is a **new license** request (no existing license) - a menu opens with 2 date pickers: start date, end date
    - if this is a **renewal** request (existing license present) - a menu opens with a single date picker: new expiry date (the license's start date doesn't change, only its expiry moves)
  - reject
    - when clicked, a menu opens with a text box: reason (optional)
- details that should appear on the page
  - requested by
  - existing license (link to the existing license, if one exists)
  - notes (details from the client)

### Licensing - customer backoffice (for logged-in clients)

#### Pages

1. My Licenses - list of licenses
2. License details - details about a license
3. My License Requests - list of license requests
4. Users - list of users

#### **My Licenses**

- list with all the licenses
  - product
  - status
  - expiry date
  - time left until expiry
  - "Request Renewal" button - only appears when the license is expired or near expiry
- "Request License" button

clicking on a row => opens License Details
clicking on Request Renewal/Request License opens a form (over the current screen)

1. Product (if Request Renewal was clicked, this is prepopulated with the product it was clicked from)
2. Notes - text box where the client adds notes

#### License Details (this could maybe be a pop-up like the one from My License Requests, so there aren't too many pages for clients)

- same details as the ones that appear on the My Licenses page
- license history section (inspired by the history from the admin backoffice's License Details)
- Request Renewal button - appears the same way as above

#### My License Requests

- list with all the licensing requests
  - product
  - requested on
  - requested by
  - status
  - delete button (cancel request) - only shown while status is pending

when you click on a row, a pop-up with details opens

1. product
2. requested on
3. requested by
4. notes (notes from the client)
5. status
6. rejection reason (only appears if the status is rejected)
7. delete/cancel button - only shown while status is pending

#### Users

- invite new user button (behavior like Invite Users from the admin backoffice)
- list with all the users in this org
  - name
  - email
  - status
    1. active
    2. deactivated
    3. invite sent
  - role
    1. admin
    2. reader
  - "Edit" button
  - "Deactivate" button
  - "Delete" button

clicking the edit button opens a form (over the current screen)

1. name - for when a user's name needs to change
2. email - for when the email changes??? (maybe doesn't make sense, idk)
3. role

### Additional pages

- Not found
  - page that says this page wasn't found; also shown instead of a dedicated "Forbidden" page when a logged-in user hits something they don't have access to, so unauthorized users can't tell whether a resource exists but is just blocked, or doesn't exist at all

### Additional elements

#### Header

- EnsyLabs logo?
- toggle between dark and light mode
- current identity (details about who is logged in) - maybe this is just a user icon + a name, and clicking it shows what's below
  - name
  - email
  - role
  - logout button

#### Sidebar

sidebar for navigating from one page to another

- admin backoffice
  - Products
  - Organizations
  - Licenses
  - License Requests (this one could also have a badge with a number of requests)
- customer backoffice
  - my licenses
  - my license requests
  - users

ideally this sidebar should be collapsible, and when collapsed only the icons should show (+ that badge from License Requests)

#### Error message toast

a toast that appears at the bottom of the page when an error occurs, it has a "loading bar" until it closes and an x button; on hover the loading bar pauses
