# AI Document Generation - User Guide

## Overview

The Tender Automation Platform includes powerful AI document generation capabilities powered by Anthropic's Claude API. This feature can automatically generate professional tender documents based on your requirements.

## Supported Document Types

1. **Technical Solution & Approach** - Comprehensive technical solution addressing tender requirements (1500-2000 words)
2. **Compliance Declaration** - Formal legal declaration with standard compliance clauses (300-500 words)
3. **Covering Letter** - Professional covering letter for tender submission (300-400 words)
4. **Executive Summary** - High-level summary highlighting key value propositions (400-600 words)
5. **Project Methodology** - Detailed methodology with phase-wise breakdown (800-1200 words)
6. **Complete Proposal** - Full proposal combining multiple sections

## How It Works

### 1. Provide Context

The AI needs context about your tender and company to generate relevant content:

```json
{
  "tender_title": "Implementation of Smart City Platform",
  "tender_reference": "TENDER/2024/SC/001",
  "issuing_authority": "Smart City Development Authority",
  "company_name": "TechSolutions Pvt Ltd",
  "company_profile": "Leading smart city solutions provider with 15+ years experience in urban technology",
  "experience": "Successfully deployed smart city projects in 20+ cities across India",
  "requirements": [
    "IoT sensor network deployment",
    "Real-time data analytics platform",
    "Citizen mobile application",
    "Command and control center"
  ],
  "scope_of_work": "Design, develop, deploy and maintain an integrated smart city platform with IoT sensors, analytics, and citizen services"
}
```

### 2. Choose Model

Select the Claude model based on your needs:

- **claude-sonnet-4** (Recommended) - Best balance of quality and cost ($3/$15 per 1M tokens)
- **claude-opus-4** - Highest quality, higher cost ($15/$75 per 1M tokens)
- **claude-opus-4-5** - Latest and most capable

### 3. Generate Document

Make a POST request to `/api/v1/ai/generate`:

```bash
curl -X POST "http://localhost:8000/api/v1/ai/generate" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "generation_type": "technical_solution",
    "context": {
      "tender_title": "Smart City Platform",
      "issuing_authority": "Smart City Authority",
      "company_name": "TechSolutions Pvt Ltd",
      "company_profile": "Smart city solutions provider",
      "requirements": ["IoT deployment", "Analytics platform"],
      "scope_of_work": "Integrated smart city platform"
    },
    "model": "claude-sonnet-4",
    "max_tokens": 4000,
    "temperature": 0.7,
    "save_as_document": true,
    "document_name": "Technical Solution - Smart City"
  }'
```

### 4. Get Formatted DOCX

The AI will:
1. Generate tailored content using Claude API
2. Format it into a professional Word document
3. Add company letterhead and footer
4. Apply proper heading styles and formatting
5. Save to your document library

## API Endpoints

### Generate Document

```http
POST /api/v1/ai/generate
```

**Request Body:**
```json
{
  "generation_type": "technical_solution|compliance_declaration|covering_letter|executive_summary|methodology|proposal",
  "context": {
    "tender_title": "string (required)",
    "tender_reference": "string (optional)",
    "issuing_authority": "string (required)",
    "company_name": "string (required)",
    "company_profile": "string (optional)",
    "experience": "string (optional)",
    "requirements": ["string"] or "string" (required),
    "scope_of_work": "string (required)"
  },
  "model": "claude-sonnet-4",
  "max_tokens": 4000,
  "temperature": 0.7,
  "save_as_document": true,
  "document_name": "Optional custom name"
}
```

**Response:**
```json
{
  "id": "uuid",
  "generation_type": "technical_solution",
  "content": "Generated text content...",
  "tokens_used": 3542,
  "model_used": "claude-sonnet-4",
  "document_id": "uuid (if saved)",
  "created_at": "2024-02-24T10:30:00Z"
}
```

### View Generation History

```http
GET /api/v1/ai/history?limit=50
```

Returns up to 100 most recent generations for your tenant.

### Get Usage Statistics

```http
GET /api/v1/ai/usage
```

**Response:**
```json
{
  "total_generations": 145,
  "total_tokens_used": 523450,
  "estimated_cost_usd": 15.67,
  "generations_by_type": {
    "technical_solution": 50,
    "compliance_declaration": 45,
    "covering_letter": 30,
    "executive_summary": 20
  },
  "current_month_tokens": 125300,
  "current_month_cost_usd": 3.76
}
```

### List Available Templates

```http
GET /api/v1/ai/templates
```

Returns template descriptions for each generation type.

### Get Template Details

```http
GET /api/v1/ai/template/{generation_type}
```

Returns the raw template text and required context fields.

### Preview Prompt

```http
POST /api/v1/ai/preview?generation_type=technical_solution
```

**Request Body:**
```json
{
  "tender_title": "...",
  "company_name": "...",
  ...
}
```

**Response:**
```json
{
  "generation_type": "technical_solution",
  "formatted_prompt": "Full prompt that will be sent to Claude...",
  "estimated_input_tokens": "850"
}
```

## Cost Management

### Token Tracking

- All generations are tracked with exact token counts
- Usage statistics available per tenant
- Monthly cost estimates provided

### Cost Estimates (as of Feb 2024)

| Model | Input (per 1M tokens) | Output (per 1M tokens) |
|-------|----------------------|------------------------|
| Claude Sonnet 4 | $3.00 | $15.00 |
| Claude Opus 4 | $15.00 | $75.00 |

**Example Costs:**
- Technical Solution (1500 words): ~$0.05 - $0.15
- Covering Letter (300 words): ~$0.01 - $0.03
- Complete Proposal (3000 words): ~$0.10 - $0.30

### Best Practices

1. **Start with Sonnet** - Use Claude Sonnet 4 for most documents (90% quality at 20% cost)
2. **Use Opus for Critical Docs** - Reserve Claude Opus 4 for final proposals or complex requirements
3. **Optimize Context** - Provide concise but complete context (more context = more tokens = higher cost)
4. **Reuse Generations** - Edit and refine generated content instead of regenerating
5. **Monitor Usage** - Check `/api/v1/ai/usage` regularly to track costs

## Document Formatting

### Letterhead

All generated documents include:
- Company name in header (centered, bold, blue)
- Horizontal separator line
- Page footer with company name and generation date

### Document Structure

**Technical Solution:**
- Main heading: "Technical Solution & Approach"
- Numbered sections (1, 1.1, 1.2, etc.)
- Professional formatting with justified text
- Clear headings hierarchy

**Covering Letter:**
- Business letter format
- Company letterhead
- Recipient address placeholder
- Professional sign-off section

**Compliance Declaration:**
- "DECLARATION" heading (centered, bold)
- Numbered compliance points
- Signature table at the end with fields for:
  - Authorized Signatory
  - Name
  - Designation
  - Date
  - Company Stamp

## Advanced Features

### Custom Prompts (Coming Soon)

Future versions will support:
- Custom prompt templates
- Industry-specific templates (Healthcare, Infrastructure, IT, etc.)
- Multi-language generation
- Template versioning

### Integration with Tender Management

Generated documents can be:
- Automatically linked to specific tenders
- Included in PDF assembly workflows
- Tagged and categorized
- Version controlled

## Troubleshooting

### Error: "Claude API error: Invalid API key"

**Solution:** Set the `ANTHROPIC_API_KEY` environment variable:
```bash
export ANTHROPIC_API_KEY="sk-ant-api03-..."
```

### Error: "Failed to generate document: Missing required context"

**Solution:** Ensure all required context fields are provided:
- `tender_title`
- `issuing_authority`
- `company_name`
- `requirements`
- `scope_of_work`

### Generated Content is Too Generic

**Solutions:**
1. Provide more detailed context (company profile, specific experience)
2. List specific requirements instead of generic ones
3. Use higher temperature (0.8-1.0) for more creative output
4. Try Claude Opus 4 for better quality

### High Token Usage / Costs

**Solutions:**
1. Reduce `max_tokens` (default: 4000, try 2000-3000)
2. Use more concise context descriptions
3. Switch to Claude Sonnet 4 instead of Opus
4. Reuse and edit existing generations

## Examples

### Example 1: Technical Solution for Smart City Project

```python
import requests

response = requests.post(
    "http://localhost:8000/api/v1/ai/generate",
    headers={"Authorization": f"Bearer {token}"},
    json={
        "generation_type": "technical_solution",
        "context": {
            "tender_title": "Smart City Command and Control Center",
            "issuing_authority": "Municipal Corporation of Greater Mumbai",
            "company_name": "SmartTech Solutions Pvt Ltd",
            "company_profile": "Leading provider of smart city solutions with 100+ deployments across India, specializing in IoT, AI, and urban analytics",
            "experience": "Successfully deployed command centers in Delhi, Bangalore, Pune with 99.9% uptime. ISO 27001 certified with experience in government IT projects",
            "requirements": [
                "Integrated IoT sensor network across 500 sq km",
                "Real-time video analytics with AI-powered incident detection",
                "Citizen grievance mobile app with geo-tagging",
                "Traffic management system with adaptive signals",
                "Environmental monitoring (air quality, noise, weather)",
                "24/7 control room with video wall and GIS mapping"
            ],
            "scope_of_work": "Design, supply, install, commission, and maintain an integrated smart city command and control center with IoT infrastructure, analytics platform, and citizen services for 5 years including AMC"
        },
        "model": "claude-sonnet-4",
        "save_as_document": true
    }
)

print(f"Generated document: {response.json()['document_id']}")
print(f"Tokens used: {response.json()['tokens_used']}")
```

### Example 2: Compliance Declaration

```python
response = requests.post(
    "http://localhost:8000/api/v1/ai/generate",
    headers={"Authorization": f"Bearer {token}"},
    json={
        "generation_type": "compliance_declaration",
        "context": {
            "tender_title": "Supply of Medical Equipment",
            "tender_reference": "MED/2024/Q1/042",
            "issuing_authority": "All India Institute of Medical Sciences (AIIMS)",
            "company_name": "MediEquip India Pvt Ltd"
        },
        "model": "claude-sonnet-4",
        "temperature": 0.5,  # Lower temperature for formal documents
        "save_as_document": true,
        "document_name": "Compliance Declaration - AIIMS Medical Equipment"
    }
)
```

## Next Steps

1. **Set up API key**: Get your Anthropic API key from https://console.anthropic.com/
2. **Test locally**: Run `python test_ai_generation.py` to verify DOCX generation
3. **Start the server**: `uvicorn app.main:app --reload`
4. **Generate your first document**: Use the examples above
5. **Build the UI**: Integrate with Angular frontend for user-friendly interface

## Support

For issues or questions:
- Check `/api/v1/docs` for interactive API documentation
- Review generated DOCX files for formatting quality
- Monitor usage stats at `/api/v1/ai/usage`
- Contact support if costs exceed expectations
