"""
Standard Indian Tender Declaration Templates.

15 deterministic templates that require only company + tender data substitution.
No AI API calls needed — instant generation.

Placeholders used:
  {company_name}, {pan_number}, {gstin}, {registered_address},
  {tender_title}, {tender_reference}, {issuing_authority},
  {date}, {authorized_signatory}, {designation},
  {msme_registration}, {cin_number}, {website}, {phone}, {email}
"""

DECLARATION_TEMPLATES = [
    {
        "key": "non_blacklist",
        "name": "Non-Blacklisting / Non-Debarment Declaration",
        "category": "eligibility",
        "description": "Declares that the company has not been blacklisted or debarred by any government agency.",
        "content_template": """DECLARATION OF NON-BLACKLISTING / NON-DEBARMENT

To,
{issuing_authority}

Subject: Declaration of Non-Blacklisting in reference to Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby solemnly declare and affirm as under:

1. That our company / firm / organization has not been blacklisted or debarred by any Central Government Ministry/Department, State Government, Public Sector Undertaking (PSU), Autonomous Body, or any other Government Agency in India or abroad, as on the date of submission of this tender.

2. That no case of fraud, malpractice, or any criminal proceeding is pending against our company / firm / any of its Directors/Partners in any court of law in India or abroad.

3. That our company / firm has not been declared insolvent or bankrupt by any competent authority.

4. That we have not been convicted of any offense involving moral turpitude or economic offense.

5. That no investigation by any investigating agency (CBI/CVC/State Anti-Corruption Bureau or equivalent) is pending against our company / firm / any of its Directors/Partners.

6. We understand that if this declaration is found to be false at any stage, the tender/contract shall be liable for cancellation/termination and our company shall be liable for blacklisting/debarment and any other action as deemed fit by the procuring authority.

This declaration is made on {date} and is true to the best of our knowledge and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "make_in_india",
        "name": "Make in India Declaration",
        "category": "compliance",
        "description": "Declaration of compliance with Make in India policy and local content requirements.",
        "content_template": """MAKE IN INDIA DECLARATION
(As per Public Procurement (Preference to Make in India) Order 2017 and amendments thereof)

To,
{issuing_authority}

Subject: Make in India Declaration for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare that:

1. We are a Class-I Local Supplier / Class-II Local Supplier (strike out whichever is not applicable) as defined under the Public Procurement (Preference to Make in India) Order, 2017 and its subsequent amendments.

2. The local content in the goods/services offered by us against Tender No. {tender_reference} - "{tender_title}" is ______% (to be filled by bidder).

3. The local content has been calculated as per the methodology prescribed in the above-mentioned order.

4. We undertake to provide the details of local content computation and any relevant supporting documents as and when required by the procuring entity for verification purposes.

5. We understand that the local content indicated above is subject to verification and any false declaration will result in:
   a. Rejection of our bid
   b. Forfeiture of EMD/Performance Security
   c. Debarment from future procurement for a period of up to three years
   d. Any other penal action as prescribed under the order

6. We confirm that we shall maintain records/documents to substantiate the local content claim and make them available for inspection/audit at any time during the contract period and for a period of three years thereafter.

This declaration is true and correct to the best of our knowledge and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "land_border",
        "name": "Land Border Country Declaration (China Clause)",
        "category": "compliance",
        "description": "Declaration regarding bidders from countries sharing land border with India (Order No. 6040/2020).",
        "content_template": """DECLARATION REGARDING COUNTRIES SHARING LAND BORDER WITH INDIA
(As per OM No. 6/18/2019-PPD dated 23.07.2020 and Order (Public Procurement No. 1) dated 04.06.2020)

To,
{issuing_authority}

Subject: Certificate for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby certify the following:

(Select whichever is applicable)

[ ] OPTION A: We are not from a country which shares a land border with India.

[ ] OPTION B: We are from a country which shares a land border with India and have been registered with the Competent Authority. The registration details are as follows:
    Registration Number: ___________________
    Competent Authority: ___________________
    Date of Registration: ___________________

We hereby certify that {company_name} fulfills all requirements in this regard and is eligible to be considered for this procurement.

Note: Countries sharing land borders with India include: China, Pakistan, Bangladesh, Myanmar, Nepal, Bhutan, and Afghanistan.

We understand that if this declaration is found to be false, the bid shall be rejected and the bidder shall be liable for action as per the provisions of the applicable orders, rules, and regulations.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "no_relation",
        "name": "No-Relation Certificate",
        "category": "eligibility",
        "description": "Declares no relation with any employee of the procuring organization.",
        "content_template": """CERTIFICATE OF NO RELATION

To,
{issuing_authority}

Subject: Certificate of No Relation for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby certify and declare that:

1. None of the Directors / Partners / Proprietor / Key Managerial Personnel of our company / firm is related to any officer or employee of {issuing_authority} or any member of the Tender Evaluation Committee.

2. None of the employees of {issuing_authority} has any financial interest, directly or indirectly, in our company / firm.

3. We have not employed, nor shall we employ, any serving or retired officer/employee of {issuing_authority} who was associated with the project/procurement for which this tender is being submitted, without prior written approval from the appropriate authority.

4. We understand that any breach of this declaration may lead to cancellation of the bid/contract and debarment from future tenders.

This certificate is issued in connection with our bid submitted against Tender No. {tender_reference} - "{tender_title}".

This declaration is true to the best of our knowledge and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "no_conviction",
        "name": "Non-Conviction Certificate",
        "category": "eligibility",
        "description": "Declares no criminal convictions against the company or its directors.",
        "content_template": """NON-CONVICTION CERTIFICATE

To,
{issuing_authority}

Subject: Non-Conviction Certificate for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby solemnly affirm and declare that:

1. Our company / firm and its Directors / Partners / Proprietor have not been convicted by any court of law in India or abroad for any criminal offense, including but not limited to offenses involving fraud, corruption, moral turpitude, or economic crimes.

2. No criminal case / charge sheet / FIR is currently pending against our company / firm or any of its Directors / Partners / Proprietor in any court of law.

3. Our company / firm has not been found guilty of any violation of the Prevention of Corruption Act, 1988 or the Indian Penal Code relating to fraud, cheating, or criminal breach of trust.

4. No Director / Partner / Proprietor of our company / firm is a convicted offender or an undischarged insolvent.

5. We understand that if at any stage, this declaration is found to be false or misleading, the procuring entity shall have the right to:
   a. Reject our bid / cancel the contract
   b. Forfeit our EMD / performance security
   c. Debar our company from future procurements
   d. Initiate legal proceedings as deemed necessary

This declaration is made on {date} and is true and correct to the best of our knowledge, information, and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "authenticity",
        "name": "Authenticity of Documents Declaration",
        "category": "compliance",
        "description": "Declares all submitted documents and information are genuine and authentic.",
        "content_template": """DECLARATION OF AUTHENTICITY OF DOCUMENTS

To,
{issuing_authority}

Subject: Declaration of Authenticity of Documents for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby solemnly declare and certify that:

1. All the documents, certificates, testimonials, and information furnished by us along with our bid against Tender No. {tender_reference} - "{tender_title}" are genuine, authentic, and true in all respects.

2. The photocopies / scanned copies of all documents submitted are true copies of the originals and have not been tampered with or altered in any manner.

3. All the information provided in the bid documents, including technical specifications, financial statements, past experience certificates, and other credentials, is factually correct and verifiable.

4. The original documents shall be produced for verification as and when demanded by the procuring entity.

5. We understand that if at any stage, any document or information submitted by us is found to be false, fabricated, tampered with, or misleading:
   a. Our bid shall be summarily rejected
   b. The EMD / performance security shall be forfeited
   c. The contract, if awarded, shall be terminated
   d. Our firm shall be blacklisted / debarred from future procurements
   e. Legal action shall be initiated against us as per applicable laws

6. We accept full responsibility for the authenticity of all documents submitted.

This declaration is made on {date} and is true to the best of our knowledge, information, and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "acceptance_terms",
        "name": "Acceptance of Terms & Conditions",
        "category": "compliance",
        "description": "Unconditional acceptance of all tender terms and conditions.",
        "content_template": """DECLARATION OF ACCEPTANCE OF TERMS AND CONDITIONS

To,
{issuing_authority}

Subject: Unconditional Acceptance of Terms and Conditions for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare and confirm that:

1. We have carefully read and understood all the terms and conditions, instructions to bidders, scope of work, technical specifications, and all other documents forming part of Tender No. {tender_reference} - "{tender_title}".

2. We hereby unconditionally and irrevocably accept all the terms, conditions, clauses, and stipulations contained in the tender documents without any deviation, reservation, or condition.

3. We shall comply with all the provisions of the tender documents and shall perform the contract in accordance with the terms and conditions specified therein.

4. We understand and accept that:
   a. The decision of the procuring entity shall be final and binding in all matters relating to the tender
   b. The procuring entity reserves the right to accept or reject any or all bids without assigning any reason
   c. Any additional terms or conditions proposed by us that are not part of the tender documents shall be deemed null and void

5. We confirm that our bid is valid for the period specified in the tender documents and may be accepted at any time during that period.

6. We further declare that there are no conditions or qualifications attached to this acceptance.

This declaration is made on {date} and is valid and binding upon us.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "msme_declaration",
        "name": "MSME Declaration / Udyam Certificate",
        "category": "eligibility",
        "description": "Declaration of MSME status under Udyam Registration for benefits and exemptions.",
        "content_template": """MSME / UDYAM REGISTRATION DECLARATION

To,
{issuing_authority}

Subject: MSME Declaration for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare that:

1. Our enterprise is registered as a Micro / Small / Medium Enterprise (strike out whichever is not applicable) under the Micro, Small and Medium Enterprises Development (MSMED) Act, 2006.

2. Our Udyam Registration details are as follows:
   Udyam Registration Number: {msme_registration}
   Date of Registration: ___________________
   Category: Micro / Small / Medium Enterprise
   Type: Manufacturing / Service (strike out whichever is not applicable)

3. We are eligible for the benefits/preferences available to MSMEs under:
   a. Public Procurement Policy for Micro and Small Enterprises (MSEs) Order, 2012
   b. Revised Public Procurement Policy dated 09.11.2018
   c. Any other applicable government orders/notifications

4. We claim the following benefits as applicable:
   [ ] Exemption from payment of Earnest Money Deposit (EMD)
   [ ] Exemption from prior turnover and experience criteria
   [ ] Price preference as per applicable policy
   [ ] Any other benefits as per policy

5. The information provided above is true and correct. We understand that furnishing false information may result in cancellation of our registration and penal action.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "gst_compliance",
        "name": "GST Compliance Declaration",
        "category": "financial",
        "description": "Declaration confirming GST registration and regular filing of returns.",
        "content_template": """GST COMPLIANCE DECLARATION

To,
{issuing_authority}

Subject: GST Compliance Certificate for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare and certify that:

1. Our company / firm is duly registered under the Goods and Services Tax (GST) Act, 2017.

2. Our GST details are as follows:
   GSTIN: {gstin}
   State of Registration: ___________________
   Type of Registration: Regular / Composition (strike out whichever is not applicable)
   Date of Registration: ___________________

3. We hereby confirm that:
   a. All GST returns (GSTR-1, GSTR-3B, GSTR-9/9C as applicable) have been filed regularly and are up-to-date as on the date of submission of this bid.
   b. There are no outstanding tax demands or penalties under GST against our company / firm.
   c. No proceedings for assessment, reassessment, or investigation are pending under GST.
   d. Our GST registration is active and has not been cancelled or suspended.

4. We undertake to:
   a. Issue proper GST-compliant tax invoices for all supplies
   b. Charge GST at the applicable rates and remit the same to the government
   c. File all GST returns regularly throughout the contract period
   d. Provide GST-compliant documentation as required by the procuring entity

5. We understand that any non-compliance with GST provisions may result in withholding of payments and other penal consequences.

PAN: {pan_number}""",
    },
    {
        "key": "annual_turnover",
        "name": "Annual Turnover Certificate",
        "category": "financial",
        "description": "Self-declaration of annual turnover for the past three financial years.",
        "content_template": """ANNUAL TURNOVER CERTIFICATE / SELF-DECLARATION

To,
{issuing_authority}

Subject: Annual Turnover Declaration for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby certify and declare our annual turnover for the last three financial years as under:

| Financial Year | Annual Turnover (INR) | Net Profit (INR) |
|----------------|----------------------|-------------------|
| 2023-24        | _______________      | _______________   |
| 2022-23        | _______________      | _______________   |
| 2021-22        | _______________      | _______________   |

Average Annual Turnover (3 years): INR _______________

We further declare that:

1. The above financial figures are as per our audited financial statements and are true and correct.

2. Our company / firm has not been declared sick or is not under winding-up proceedings.

3. We have positive net worth as on the date of submission of this bid.

4. The audited financial statements / balance sheets for the above-mentioned years are enclosed / shall be produced for verification as and when required.

5. We understand that any misrepresentation of financial data shall lead to disqualification and legal action.

PAN: {pan_number}
GSTIN: {gstin}
CIN: {cin_number}""",
    },
    {
        "key": "oem_authorization",
        "name": "OEM Authorization Letter Template",
        "category": "technical",
        "description": "Template for OEM authorization letter to be issued by the manufacturer.",
        "content_template": """OEM AUTHORIZATION LETTER

(To be issued on the letterhead of the Original Equipment Manufacturer)

Date: {date}
Reference: {tender_reference}

To,
{issuing_authority}

Subject: OEM Authorization for Tender No. {tender_reference} - "{tender_title}"

Dear Sir/Madam,

We, [OEM Company Name], having our registered office at [OEM Address], being the Original Equipment Manufacturer (OEM) of [Product/Equipment Name], do hereby authorize {company_name} (registered at {registered_address}) to quote and supply our products/equipment against the above-referenced tender.

We confirm that:

1. {company_name} is our authorized partner / dealer / distributor / system integrator (strike out whichever is not applicable) for the products mentioned in this tender.

2. The products quoted by {company_name} are genuine products manufactured by us and meet the specifications mentioned in the tender document.

3. We shall provide:
   a. Warranty support for a period of _______ years from the date of installation/commissioning
   b. Technical support and after-sales service through our authorized service network
   c. Supply of spare parts for a minimum period of _______ years from the date of supply

4. In case {company_name} fails to perform its obligations under the contract, we shall step in and fulfill the contractual obligations.

5. This authorization is valid for the purpose of this specific tender only.

For [OEM Company Name]

___________________________
Authorized Signatory
Name:
Designation:
Date:
Company Seal:""",
    },
    {
        "key": "no_deviation",
        "name": "No-Deviation / No-Assumption Statement",
        "category": "compliance",
        "description": "States no deviations or assumptions from the tender specifications.",
        "content_template": """NO-DEVIATION / NO-ASSUMPTION STATEMENT

To,
{issuing_authority}

Subject: No-Deviation / No-Assumption Declaration for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare and confirm that:

1. We have thoroughly studied and understood the complete tender documents including all addenda/corrigenda (if any) issued by the procuring entity for Tender No. {tender_reference} - "{tender_title}".

2. Our bid is strictly in conformity with the tender requirements and there are NO deviations whatsoever from the terms and conditions, technical specifications, scope of work, delivery schedule, or any other requirements specified in the tender documents.

3. We have NOT made any assumptions, either expressed or implied, regarding the scope of work, technical specifications, site conditions, or any other aspects of the project.

4. We confirm that:
   a. All items quoted by us fully comply with the specifications mentioned in the tender
   b. We have not proposed any alternative materials, brands, or specifications
   c. We have not attached any separate terms or conditions to our bid
   d. Our bid prices are inclusive of all costs as specified in the tender documents

5. We understand that any deviation found in our bid documents shall be treated as non-responsive, and the procuring entity reserves the right to reject our bid on that ground.

6. In case of any inadvertent deviation found at any stage, the terms and conditions of the tender shall prevail and shall be binding upon us.

This declaration is made on {date} and forms an integral part of our bid.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "bid_validity",
        "name": "Bid Validity Declaration",
        "category": "compliance",
        "description": "Declares bid validity period and commitment to maintain prices.",
        "content_template": """BID VALIDITY DECLARATION

To,
{issuing_authority}

Subject: Bid Validity Declaration for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare and confirm that:

1. Our bid submitted against Tender No. {tender_reference} - "{tender_title}" shall remain valid and binding upon us for a period of _______ days from the date of opening of the bid (or as specified in the tender document).

2. We shall not withdraw, modify, or revise our bid during the validity period without the written consent of the procuring entity.

3. The prices quoted in our bid are firm and fixed for the entire validity period and shall not be subject to any escalation, variation, or adjustment on account of:
   a. Fluctuation in foreign exchange rates
   b. Changes in customs duty, taxes, or levies (except GST rate changes by government notification)
   c. Increase in cost of raw materials, labor, or any other input costs
   d. Any other reason whatsoever

4. In the event of our being awarded the contract, we shall extend the bid validity if required by the procuring entity for the purpose of evaluation, clarification, or contract finalization.

5. If we withdraw or modify our bid during the validity period, we understand that:
   a. Our EMD shall be forfeited
   b. We may be debarred from future procurements
   c. Any other penal action may be taken as per tender conditions

This declaration is made on {date} and is irrevocable during the bid validity period.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "conflict_of_interest",
        "name": "Conflict of Interest Declaration",
        "category": "eligibility",
        "description": "Declares no conflict of interest in the tender process.",
        "content_template": """CONFLICT OF INTEREST DECLARATION

To,
{issuing_authority}

Subject: Declaration of No Conflict of Interest for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby solemnly declare and affirm that:

1. Neither our company / firm nor any of its Directors / Partners / Proprietor / Associates / Sub-contractors have any conflict of interest, either direct or indirect, in the subject matter of Tender No. {tender_reference} - "{tender_title}".

2. We have not directly or indirectly:
   a. Been involved in the preparation of the tender specifications, terms of reference, or bill of quantities for this procurement
   b. Received any insider information that could give us an unfair advantage over other bidders
   c. Engaged in any collusion, bid rigging, or cartel formation with any other bidder(s) or any officer/employee of the procuring entity

3. We do not have any business relationship or financial interest in any other bidder participating in this tender that could be construed as a conflict of interest.

4. No former employee of the procuring entity who was involved in the preparation of this tender is currently employed by or associated with our company / firm.

5. We undertake to immediately disclose any potential conflict of interest that may arise during the tenure of the contract.

6. We understand that if any conflict of interest is discovered at any stage, the procuring entity shall have the right to disqualify our bid, terminate the contract, and debar us from future procurements.

This declaration is made on {date} and is true and correct to the best of our knowledge, information, and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "integrity_pact",
        "name": "Integrity Pact Declaration",
        "category": "compliance",
        "description": "Commitment to integrity and anti-corruption in the procurement process.",
        "content_template": """INTEGRITY PACT DECLARATION

To,
{issuing_authority}

Subject: Integrity Pact for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby solemnly declare and undertake the following:

PREAMBLE

This Integrity Pact is entered into between {issuing_authority} (hereinafter referred to as "the Buyer") and {company_name} (hereinafter referred to as "the Bidder/Seller") for Tender No. {tender_reference} - "{tender_title}".

COMMITMENTS OF THE BIDDER / SELLER

1. The Bidder commits to take all measures necessary to prevent corruption and to observe the following principles during participation in this tender and during the execution of the resulting contract:

2. The Bidder will not, directly or through any other person or firm:
   a. Offer, promise, or give to any officer or employee of the Buyer or any third person any material or immaterial benefit which he/she is not legally entitled to, in order to obtain any advantage in the procurement process
   b. Enter into any undisclosed agreement or arrangement with other bidders to manipulate the prices, quality, or the tender outcome
   c. Improperly influence the procurement process or tender evaluation

3. The Bidder will not use improperly, for purposes of competition or personal gain, any information or document provided by the Buyer as part of the business relationship.

4. The Bidder shall, when signing this Integrity Pact, disclose any and all payments made or agreed to be made relating to this specific tender and contract.

PENALTIES FOR VIOLATION

5. In case of violation of any of the above provisions:
   a. The Buyer may disqualify the Bidder from the tender process
   b. The Buyer may cancel the contract and recover all sums already paid
   c. The Buyer may forfeit the EMD / performance security
   d. The Buyer may debar the Bidder from future procurements for a suitable period
   e. The Bidder agrees and accepts that any violation may lead to legal proceedings

INDEPENDENT EXTERNAL MONITOR (IEM)

6. The Buyer may appoint one or more Independent External Monitors (IEMs) to review the procurement process. The IEM shall have access to all relevant documents and information.

VALIDITY

7. This Integrity Pact shall be valid from the date of the invitation of bids till the complete execution of the contract or until the warranty period expires, whichever is later.

This declaration is made on {date} and is binding upon us.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
]


def get_all_templates() -> list:
    """Return all declaration template metadata (without full content)."""
    return [
        {
            "key": t["key"],
            "name": t["name"],
            "category": t["category"],
            "description": t["description"],
        }
        for t in DECLARATION_TEMPLATES
    ]


def get_template_by_key(key: str) -> dict | None:
    """Return a single template by key."""
    for t in DECLARATION_TEMPLATES:
        if t["key"] == key:
            return t
    return None


def get_templates_by_keys(keys: list[str]) -> list[dict]:
    """Return templates matching the given keys, preserving order."""
    key_map = {t["key"]: t for t in DECLARATION_TEMPLATES}
    return [key_map[k] for k in keys if k in key_map]
