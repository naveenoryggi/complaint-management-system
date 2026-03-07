"""
Standard Indian Tender Declaration Templates.

30 deterministic templates that require only company + tender data substitution.
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
        "is_required": True,
        "description": "Declares that the company has not been blacklisted or debarred by any government agency.",
        "content_template": """DECLARATION OF NON-BLACKLISTING / NON-DEBARMENT

To,
{issuing_authority}

Subject: Declaration of Non-Blacklisting for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, participating as {bidder_role}, having our registered office at {registered_address}, do hereby declare that:

1. Our company has not been blacklisted or debarred by any Central/State Government Ministry, PSU, Autonomous Body, or any Government Agency in India or abroad as on date.

2. No case of fraud, malpractice, or criminal proceeding is pending against our company or its Directors/Partners.

3. Our company has not been declared insolvent or bankrupt by any competent authority.

4. If this declaration is found false at any stage, the tender/contract shall be liable for cancellation and our company shall be liable for blacklisting and other action as deemed fit by the procuring authority.

This declaration is made on {date} and is true to the best of our knowledge and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "make_in_india",
        "name": "Make in India Declaration",
        "category": "compliance",
        "is_required": True,
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
        "is_required": True,
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
        "is_required": False,
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
        "is_required": True,
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
        "is_required": True,
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
        "is_required": True,
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
        "is_required": False,
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
        "is_required": False,
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
        "is_required": False,
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
        "is_required": False,
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
        "is_required": True,
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
        "is_required": True,
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
        "is_required": True,
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
        "is_required": True,
        "description": "Commitment to integrity and anti-corruption in the procurement process.",
        "content_template": """INTEGRITY PACT DECLARATION

To,
{issuing_authority}

Subject: Integrity Pact for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare and undertake that:

1. We shall take all measures to prevent corruption and shall not directly or indirectly offer, promise, or give any undue advantage to obtain favourable treatment in this procurement.

2. We shall not enter into any undisclosed agreement with other bidders to manipulate prices, quality, or tender outcome, nor improperly influence the evaluation process.

3. We shall not misuse any information or documents provided by {issuing_authority} for competitive advantage or personal gain.

4. We acknowledge that violation of these commitments may result in disqualification, contract cancellation, forfeiture of EMD/performance security, and debarment from future procurements.

5. This Integrity Pact is valid from the date of bid invitation till complete execution of the contract or warranty expiry, whichever is later.

This declaration is made on {date} and is binding upon us.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "pf_compliance",
        "name": "Provident Fund (PF) Compliance Declaration",
        "category": "compliance",
        "is_required": False,
        "description": "Declaration of compliance with Employees' Provident Fund and Miscellaneous Provisions Act, 1952.",
        "content_template": """PROVIDENT FUND (PF) COMPLIANCE DECLARATION

To,
{issuing_authority}

Subject: PF Compliance Declaration for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare that:

1. Our establishment is registered under the Employees' Provident Funds and Miscellaneous Provisions Act, 1952.
   PF Establishment Code: {pf_registration_number}

2. We are regular in depositing PF contributions and all returns (ECR) are up-to-date. There are no outstanding PF dues or penalties against our establishment.

3. We undertake to comply with all PF provisions throughout the contract period and provide compliance certificates as required.

4. Non-compliance with PF provisions may result in withholding of payments and other penal consequences.

This declaration is made on {date} and is true and correct.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "esi_compliance",
        "name": "ESI (Employees' State Insurance) Compliance Declaration",
        "category": "compliance",
        "is_required": False,
        "description": "Declaration of compliance with the Employees' State Insurance Act, 1948.",
        "content_template": """ESI COMPLIANCE DECLARATION

To,
{issuing_authority}

Subject: ESI Compliance Declaration for Tender No. {tender_reference}

Dear Sir/Madam,

We, {company_name}, having our registered office at {registered_address}, do hereby declare that:

1. Our establishment is registered under the Employees' State Insurance Act, 1948.
   ESI Code Number: {esi_registration_number}

2. We are regular in depositing ESI contributions and all returns are up-to-date. There are no outstanding ESI dues or penalties against our establishment.

3. We undertake to comply with all ESI provisions throughout the contract period and provide compliance certificates as required.

4. Non-compliance with ESI provisions may result in withholding of payments and other penal consequences.

This declaration is made on {date} and is true and correct.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "power_of_attorney",
        "name": "Power of Attorney",
        "category": "eligibility",
        "is_required": False,
        "description": "Authorizes a representative to sign bid documents on behalf of the company.",
        "content_template": """POWER OF ATTORNEY

To,
{issuing_authority}

Subject: Power of Attorney for Tender No. {tender_reference}

Know all men by these presents, We, {company_name} ({role_abbrev}), a company incorporated under the laws of India, having its registered office at {registered_address}, do hereby appoint and authorize {authorized_signatory}, {designation}, as our attorney and authorized representative to:

1. Sign, submit, and execute all documents relating to Tender No. {tender_reference} - "{tender_title}" on behalf of {company_name}.

2. Attend pre-bid meetings, tender opening events, and negotiate on our behalf.

3. Receive correspondence and communications from {issuing_authority} related to this tender.

4. Bind the company in all matters related to this procurement process.

This Power of Attorney is valid for the duration of the tender process and the resulting contract period.

PAN: {pan_number}
GSTIN: {gstin}
CIN: {cin_number}""",
    },
    {
        "key": "no_litigation",
        "name": "No Pending Litigation Declaration",
        "category": "eligibility",
        "is_required": False,
        "description": "Declares no pending litigation that could affect contract performance.",
        "content_template": """DECLARATION OF NO PENDING LITIGATION

To,
{issuing_authority}

Subject: No Pending Litigation Declaration for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby declare that:

1. There is no pending litigation or arbitration proceeding against our company that could materially affect our ability to perform the contract arising from Tender No. {tender_reference}.

2. No winding-up petition or insolvency proceeding has been filed against our company.

3. We are not involved in any legal dispute with any government entity that could impair our eligibility for this tender.

4. We undertake to inform {issuing_authority} immediately if any such litigation or proceeding arises during the tender or contract period.

This declaration is made on {date} and is true to the best of our knowledge and belief.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "past_performance",
        "name": "Past Performance / Experience Declaration",
        "category": "technical",
        "is_required": False,
        "description": "Self-declaration of relevant past experience and completed projects.",
        "content_template": """PAST PERFORMANCE / EXPERIENCE DECLARATION

To,
{issuing_authority}

Subject: Experience Declaration for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby declare that:

1. We have successfully executed similar contracts/projects during the last ______ years as detailed below:

| S.No | Client Name | Project Description | Contract Value (INR) | Period | Completion Status |
|------|-------------|--------------------|--------------------|--------|------------------|
| 1    |             |                    |                    |        |                  |
| 2    |             |                    |                    |        |                  |
| 3    |             |                    |                    |        |                  |

2. Completion certificates / work orders for the above projects are enclosed / shall be produced upon request.

3. We confirm that no contract awarded to us has been terminated for default or poor performance during the last five years.

This declaration is made on {date} and is true and correct.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "manpower_declaration",
        "name": "Manpower Deployment Declaration",
        "category": "technical",
        "is_required": False,
        "description": "Commitment to deploy qualified manpower as per tender requirements.",
        "content_template": """MANPOWER DEPLOYMENT DECLARATION

To,
{issuing_authority}

Subject: Manpower Declaration for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby declare and undertake that:

1. We have adequate qualified manpower to execute the scope of work as specified in Tender No. {tender_reference}.

2. We shall deploy only qualified and experienced personnel as per the requirements of the tender.

3. The key personnel proposed shall not be changed without prior written approval of {issuing_authority}.

4. We shall comply with all applicable labour laws including minimum wages, working hours, and safety regulations.

This declaration is made on {date} and is true and correct.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "subcontracting",
        "name": "No Subcontracting Declaration",
        "category": "compliance",
        "is_required": False,
        "description": "Commitment not to subcontract without prior approval.",
        "content_template": """NO SUBCONTRACTING DECLARATION

To,
{issuing_authority}

Subject: No Subcontracting Declaration for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby declare that:

1. We shall not subcontract, assign, or transfer the contract or any part thereof to any third party without the prior written approval of {issuing_authority}.

2. We possess the required infrastructure, resources, and capability to execute the entire scope of work independently.

3. In case subcontracting is permitted by the tender, we shall remain solely responsible for the quality, timelines, and obligations under the contract.

This declaration is made on {date} and forms part of our bid.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "warranty_commitment",
        "name": "Warranty / AMC Commitment Declaration",
        "category": "technical",
        "is_required": False,
        "description": "Commitment to warranty and post-contract support obligations.",
        "content_template": """WARRANTY / AMC COMMITMENT DECLARATION

To,
{issuing_authority}

Subject: Warranty Commitment for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby declare and undertake that:

1. We shall provide comprehensive warranty for the products/services supplied under Tender No. {tender_reference} for a period as specified in the tender document from the date of acceptance/commissioning.

2. During the warranty period, we shall provide free replacement/repair of any defective items and attend to complaints within the response time specified in the tender.

3. We shall maintain adequate spare parts inventory and service infrastructure for the warranty period and beyond.

4. After the warranty period, we shall offer Annual Maintenance Contract (AMC) at competitive rates if required by {issuing_authority}.

This declaration is made on {date} and is binding upon us.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "environment_compliance",
        "name": "Environmental Compliance Declaration",
        "category": "compliance",
        "is_required": False,
        "description": "Declaration of compliance with environmental laws and regulations.",
        "content_template": """ENVIRONMENTAL COMPLIANCE DECLARATION

To,
{issuing_authority}

Subject: Environmental Compliance Declaration for Tender No. {tender_reference}

We, {company_name}, having our registered office at {registered_address}, do hereby declare that:

1. Our company / manufacturing facility complies with all applicable environmental laws and regulations including the Environment Protection Act, 1986 and rules made thereunder.

2. We hold all necessary environmental clearances and consent orders from the State/Central Pollution Control Board (as applicable).

3. No penalty or show-cause notice for environmental violation is pending against our company.

4. We shall adhere to all environmental norms during the execution of the contract.

This declaration is made on {date} and is true and correct.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "data_security",
        "name": "Data Security / Confidentiality Declaration",
        "category": "compliance",
        "is_required": False,
        "description": "Commitment to data security and confidentiality of procuring entity's information.",
        "content_template": """DATA SECURITY / CONFIDENTIALITY DECLARATION

To,
{issuing_authority}

Subject: Data Security & Confidentiality Declaration for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby declare and undertake that:

1. We shall maintain strict confidentiality of all information, data, and documents shared by {issuing_authority} during the tender process and contract execution.

2. We shall implement appropriate data security measures to protect sensitive information from unauthorized access, disclosure, or misuse.

3. We shall not use the procuring entity's data for any purpose other than fulfilling contractual obligations.

4. We shall comply with the Information Technology Act, 2000 and applicable data protection regulations.

5. Upon termination of the contract, we shall return or destroy all confidential information as directed by {issuing_authority}.

This declaration is made on {date} and remains binding throughout the contract period and thereafter.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "site_visit",
        "name": "Site Visit Certificate / Declaration",
        "category": "technical",
        "is_required": False,
        "description": "Confirmation of site visit and understanding of ground conditions.",
        "content_template": """SITE VISIT CERTIFICATE / DECLARATION

To,
{issuing_authority}

Subject: Site Visit Declaration for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby certify that:

1. Our authorized representative(s) have visited the site/location(s) mentioned in Tender No. {tender_reference} and have fully acquainted ourselves with the local conditions, site conditions, accessibility, and other factors that may affect the execution of the contract.

2. Our bid is based on our own assessment of the site conditions and we shall not claim any extra payment or variation on account of site conditions, unforeseen or otherwise.

3. We have satisfied ourselves regarding the availability of materials, labour, water, electricity, and other resources required for execution.

This declaration is made on {date} and forms part of our bid.

Date of Site Visit: ___________________
Name of Representative: ___________________

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "iso_quality",
        "name": "ISO / Quality Certification Declaration",
        "category": "technical",
        "is_required": False,
        "description": "Declaration regarding ISO and quality management certifications.",
        "content_template": """ISO / QUALITY CERTIFICATION DECLARATION

To,
{issuing_authority}

Subject: Quality Certification Declaration for Tender No. {tender_reference}

We, {company_name}, participating as {bidder_role} in this tender, having our registered office at {registered_address}, do hereby declare that:

1. Our company holds the following quality management certifications (as applicable):
   [ ] ISO 9001:2015 — Quality Management System
   [ ] ISO 14001:2015 — Environmental Management System
   [ ] ISO 27001:2013 — Information Security Management System
   [ ] ISO 45001:2018 — Occupational Health and Safety
   [ ] CMMI Level ____
   [ ] Other: ___________________

   Certificate Number: ___________________
   Certifying Body: ___________________
   Valid Until: ___________________

2. The certifications are current, valid, and have not been suspended or withdrawn.

3. Copies of the relevant certificates are enclosed / shall be produced for verification.

This declaration is made on {date} and is true and correct.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "labour_law",
        "name": "Labour Law Compliance Declaration",
        "category": "compliance",
        "is_required": False,
        "description": "Declaration of compliance with applicable labour laws and regulations.",
        "content_template": """LABOUR LAW COMPLIANCE DECLARATION

To,
{issuing_authority}

Subject: Labour Law Compliance for Tender No. {tender_reference}

We, {company_name}, having our registered office at {registered_address}, do hereby declare that:

1. We are in compliance with all applicable labour laws including but not limited to:
   a. Minimum Wages Act, 1948
   b. Contract Labour (Regulation and Abolition) Act, 1970
   c. Payment of Wages Act, 1936
   d. Payment of Bonus Act, 1965
   e. Payment of Gratuity Act, 1972
   f. Equal Remuneration Act, 1976
   g. Child Labour (Prohibition & Regulation) Act, 1986

2. We do not employ child labour or bonded labour in any form.

3. We shall comply with all labour welfare obligations during contract execution.

This declaration is made on {date} and is true and correct.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "startup_declaration",
        "name": "Startup India Declaration",
        "category": "eligibility",
        "is_required": False,
        "description": "Declaration of Startup India recognition for applicable benefits.",
        "content_template": """STARTUP INDIA DECLARATION

To,
{issuing_authority}

Subject: Startup India Declaration for Tender No. {tender_reference}

We, {company_name}, having our registered office at {registered_address}, do hereby declare that:

1. Our company is recognized as a Startup under the Startup India initiative by the Department for Promotion of Industry and Internal Trade (DPIIT).

2. Our DPIIT Recognition details are:
   Recognition Number: ___________________
   Date of Recognition: ___________________
   Valid Until: ___________________

3. We claim applicable benefits as per the public procurement policy for startups including:
   [ ] Relaxation in prior turnover criteria
   [ ] Relaxation in prior experience criteria
   [ ] Exemption from EMD

4. The information provided above is true and correct. Supporting documents are enclosed.

This declaration is made on {date}.

PAN: {pan_number}
GSTIN: {gstin}""",
    },
    {
        "key": "joint_venture",
        "name": "Joint Venture / Consortium Declaration",
        "category": "eligibility",
        "is_required": False,
        "description": "Declaration regarding joint venture or consortium participation in the tender.",
        "content_template": """JOINT VENTURE / CONSORTIUM DECLARATION

To,
{issuing_authority}

Subject: JV/Consortium Declaration for Tender No. {tender_reference}

We, {company_name} (Lead Partner), having our registered office at {registered_address}, on behalf of the Joint Venture / Consortium, do hereby declare that:

1. The following entities form this JV/Consortium for Tender No. {tender_reference}:

   Lead Partner: {company_name}
   Partner 2: ___________________
   Partner 3: ___________________

2. {company_name} shall act as the Lead Partner and single point of contact with {issuing_authority}.

3. All partners are jointly and severally liable for the performance of the contract.

4. The JV/Consortium agreement is enclosed with this bid.

5. The work share distribution among partners is as follows:
   {company_name}: ______%
   Partner 2: ______%
   Partner 3: ______%

This declaration is made on {date} and is binding on all partners.

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
            "is_required": t.get("is_required", False),
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
