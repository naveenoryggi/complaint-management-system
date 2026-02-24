"""create tender tables

Revision ID: 001
Revises:
Create Date: 2026-02-24

"""
from alembic import op
import sqlalchemy as sa
from sqlalchemy.dialects import mssql


# revision identifiers, used by Alembic.
revision = '001'
down_revision = None
branch_labels = None
depends_on = None


def upgrade() -> None:
    # Create tenders table
    op.create_table(
        'tenders',
        sa.Column('id', mssql.UNIQUEIDENTIFIER(), primary_key=True, server_default=sa.text('NEWID()')),
        sa.Column('tenant_id', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('created_by', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('title', sa.String(500), nullable=False),
        sa.Column('reference_number', sa.String(100)),
        sa.Column('issuing_authority', sa.String(300)),
        sa.Column('portal_name', sa.String(100)),
        sa.Column('portal_url', sa.Text()),
        sa.Column('deadline', sa.DateTime()),
        sa.Column('estimated_value', sa.DECIMAL(15, 2)),
        sa.Column('requirements', sa.Text(), server_default='{}'),
        sa.Column('notes', sa.Text()),
        sa.Column('status', sa.String(50), server_default='draft'),
        sa.Column('created_at', sa.DateTime(), server_default=sa.func.getdate()),
        sa.Column('updated_at', sa.DateTime(), server_default=sa.func.getdate())
    )

    # Create indexes for tenders
    op.create_index('idx_tenders_tenant', 'tenders', ['tenant_id'])
    op.create_index('idx_tenders_deadline', 'tenders', ['deadline'])
    op.create_index('idx_tenders_status', 'tenders', ['status'])

    # Create documents table
    op.create_table(
        'documents',
        sa.Column('id', mssql.UNIQUEIDENTIFIER(), primary_key=True, server_default=sa.text('NEWID()')),
        sa.Column('tenant_id', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('created_by', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('name', sa.String(255), nullable=False),
        sa.Column('description', sa.Text()),
        sa.Column('file_path', sa.String(500), nullable=False),
        sa.Column('file_size', sa.Integer, nullable=False),
        sa.Column('mime_type', sa.String(100), nullable=False),
        sa.Column('document_type', sa.String(50)),
        sa.Column('tags', sa.Text(), server_default=''),
        sa.Column('metadata', sa.Text(), server_default='{}'),
        sa.Column('is_template', sa.Boolean, server_default='0'),
        sa.Column('version', sa.Integer, server_default='1'),
        sa.Column('created_at', sa.DateTime(), server_default=sa.func.getdate()),
        sa.Column('updated_at', sa.DateTime(), server_default=sa.func.getdate())
    )

    # Create indexes for documents
    op.create_index('idx_documents_tenant', 'documents', ['tenant_id'])
    op.create_index('idx_documents_type', 'documents', ['document_type'])

    # Create tender_documents table
    op.create_table(
        'tender_documents',
        sa.Column('id', mssql.UNIQUEIDENTIFIER(), primary_key=True, server_default=sa.text('NEWID()')),
        sa.Column('tender_id', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('document_id', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('document_order', sa.Integer, server_default='0'),
        sa.Column('is_generated', sa.Boolean, server_default='0'),
        sa.Column('generation_prompt', sa.Text()),
        sa.Column('created_at', sa.DateTime(), server_default=sa.func.getdate()),
        sa.ForeignKeyConstraint(['tender_id'], ['tenders.id'], ondelete='CASCADE'),
        sa.ForeignKeyConstraint(['document_id'], ['documents.id'], ondelete='CASCADE')
    )

    # Create index for tender_documents
    op.create_index('idx_tender_docs_tender', 'tender_documents', ['tender_id'])

    # Create ai_generations table
    op.create_table(
        'ai_generations',
        sa.Column('id', mssql.UNIQUEIDENTIFIER(), primary_key=True, server_default=sa.text('NEWID()')),
        sa.Column('tenant_id', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('created_by', mssql.UNIQUEIDENTIFIER(), nullable=False),
        sa.Column('document_id', mssql.UNIQUEIDENTIFIER()),
        sa.Column('prompt', sa.Text(), nullable=False),
        sa.Column('model_used', sa.String(50), server_default='claude-sonnet-4-5'),
        sa.Column('tokens_used', sa.Integer),
        sa.Column('generation_type', sa.String(50)),
        sa.Column('input_context', sa.Text()),
        sa.Column('output_content', sa.Text()),
        sa.Column('created_at', sa.DateTime(), server_default=sa.func.getdate()),
        sa.ForeignKeyConstraint(['document_id'], ['documents.id'], ondelete='SET NULL')
    )

    # Create index for ai_generations
    op.create_index('idx_ai_generations_tenant', 'ai_generations', ['tenant_id'])


def downgrade() -> None:
    # Drop tables in reverse order
    op.drop_index('idx_ai_generations_tenant')
    op.drop_table('ai_generations')

    op.drop_index('idx_tender_docs_tender')
    op.drop_table('tender_documents')

    op.drop_index('idx_documents_type')
    op.drop_index('idx_documents_tenant')
    op.drop_table('documents')

    op.drop_index('idx_tenders_status')
    op.drop_index('idx_tenders_deadline')
    op.drop_index('idx_tenders_tenant')
    op.drop_table('tenders')
