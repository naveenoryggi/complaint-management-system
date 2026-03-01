"""Create tender automation tables

Revision ID: 001
Revises:
Create Date: 2025-02-24

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa

# revision identifiers, used by Alembic.
revision: str = '001'
down_revision: Union[str, None] = None
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    """Create all tender automation tables."""

    # Create tenders table
    op.create_table(
        'tenders',
        sa.Column('id', sa.Uuid(), nullable=False),
        sa.Column('tenant_id', sa.Uuid(), nullable=False),
        sa.Column('created_by', sa.Uuid(), nullable=False),
        sa.Column('title', sa.String(length=500), nullable=False),
        sa.Column('reference_number', sa.String(length=100), nullable=True),
        sa.Column('issuing_authority', sa.String(length=300), nullable=True),
        sa.Column('portal_name', sa.String(length=100), nullable=True),
        sa.Column('portal_url', sa.Text(), nullable=True),
        sa.Column('tender_url', sa.Text(), nullable=True),
        sa.Column('deadline', sa.DateTime(timezone=True), nullable=True),
        sa.Column('estimated_value', sa.Numeric(precision=15, scale=2), nullable=True),
        sa.Column('requirements', sa.JSON(), nullable=True),
        sa.Column('notes', sa.Text(), nullable=True),
        sa.Column('status', sa.String(length=50), nullable=False, server_default='draft'),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('GETDATE()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('GETDATE()')),
        sa.PrimaryKeyConstraint('id')
    )
    op.create_index('idx_tenders_tenant', 'tenders', ['tenant_id'])
    op.create_index('idx_tenders_deadline', 'tenders', ['deadline'])
    op.create_index('idx_tenders_status', 'tenders', ['status'])

    # Create documents table
    op.create_table(
        'documents',
        sa.Column('id', sa.Uuid(), nullable=False),
        sa.Column('tenant_id', sa.Uuid(), nullable=False),
        sa.Column('created_by', sa.Uuid(), nullable=False),
        sa.Column('name', sa.String(length=255), nullable=False),
        sa.Column('description', sa.Text(), nullable=True),
        sa.Column('file_path', sa.String(length=500), nullable=False),
        sa.Column('file_size', sa.BigInteger(), nullable=False),
        sa.Column('mime_type', sa.String(length=100), nullable=False),
        sa.Column('document_type', sa.String(length=50), nullable=True),
        sa.Column('tags', sa.JSON(), nullable=True),
        sa.Column('metadata', sa.JSON(), nullable=True),
        sa.Column('is_template', sa.Boolean(), nullable=False, server_default='0'),
        sa.Column('version', sa.Integer(), nullable=False, server_default='1'),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('GETDATE()')),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('GETDATE()')),
        sa.PrimaryKeyConstraint('id')
    )
    op.create_index('idx_documents_tenant', 'documents', ['tenant_id'])
    op.create_index('idx_documents_type', 'documents', ['document_type'])

    # Create tender_documents junction table
    op.create_table(
        'tender_documents',
        sa.Column('id', sa.Uuid(), nullable=False),
        sa.Column('tender_id', sa.Uuid(), nullable=False),
        sa.Column('document_id', sa.Uuid(), nullable=False),
        sa.Column('document_order', sa.Integer(), nullable=False, server_default='0'),
        sa.Column('is_generated', sa.Boolean(), nullable=False, server_default='0'),
        sa.Column('generation_prompt', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('GETDATE()')),
        sa.ForeignKeyConstraint(['tender_id'], ['tenders.id'], ondelete='CASCADE'),
        sa.ForeignKeyConstraint(['document_id'], ['documents.id'], ondelete='CASCADE'),
        sa.PrimaryKeyConstraint('id')
    )
    op.create_index('idx_tender_docs_tender', 'tender_documents', ['tender_id'])

    # Create ai_generations table
    op.create_table(
        'ai_generations',
        sa.Column('id', sa.Uuid(), nullable=False),
        sa.Column('tenant_id', sa.Uuid(), nullable=False),
        sa.Column('created_by', sa.Uuid(), nullable=False),
        sa.Column('document_id', sa.Uuid(), nullable=True),
        sa.Column('prompt', sa.Text(), nullable=False),
        sa.Column('model_used', sa.String(length=50), nullable=False, server_default='claude-opus-4-5'),
        sa.Column('tokens_used', sa.Integer(), nullable=True),
        sa.Column('generation_type', sa.String(length=50), nullable=True),
        sa.Column('input_context', sa.JSON(), nullable=True),
        sa.Column('output_content', sa.Text(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), nullable=False, server_default=sa.text('GETDATE()')),
        sa.ForeignKeyConstraint(['document_id'], ['documents.id'], ondelete='SET NULL'),
        sa.PrimaryKeyConstraint('id')
    )
    op.create_index('idx_ai_generations_tenant', 'ai_generations', ['tenant_id'])


def downgrade() -> None:
    """Drop all tender automation tables."""
    op.drop_index('idx_ai_generations_tenant', table_name='ai_generations')
    op.drop_table('ai_generations')
    op.drop_index('idx_tender_docs_tender', table_name='tender_documents')
    op.drop_table('tender_documents')
    op.drop_index('idx_documents_type', table_name='documents')
    op.drop_index('idx_documents_tenant', table_name='documents')
    op.drop_table('documents')
    op.drop_index('idx_tenders_status', table_name='tenders')
    op.drop_index('idx_tenders_deadline', table_name='tenders')
    op.drop_index('idx_tenders_tenant', table_name='tenders')
    op.drop_table('tenders')
