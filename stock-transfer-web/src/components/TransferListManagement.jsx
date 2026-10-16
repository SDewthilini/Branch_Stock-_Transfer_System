import React from 'react';

// Displays transfers and the actions available for each transfer status.

function TransferListManagement({ transfers, onRefresh, getBranchName, formatTransferId, getStatusBadge, getStatusString, onAction }) {
  return (
    <section className="dashboard-card transfer-card">
      <div className="dashboard-card-heading transfer-heading">
        <div className="flex items-start gap-3">
          <span className="section-icon" aria-hidden="true">⇄</span>
          <div>
            <h2 className="section-title">Transfer activity</h2>
            <p className="section-description">Review requests and update their progress.</p>
          </div>
        </div>
        <button onClick={onRefresh} className="secondary-button">
          Refresh List
        </button>
      </div>

      <div className="table-frame">
        <table className="dashboard-table">
          <thead>
            <tr>
              <th>Transfer ID</th>
              <th>From</th>
              <th>To</th>
              <th>Product</th>
              <th>Quantity</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {transfers.length > 0 ? transfers.map(transfer => {
              const currentStatus = getStatusString(transfer.status);
              return (
                <tr key={transfer.id}>
                  <td className="transfer-id">{formatTransferId(transfer.id)}</td>
                  <td>{transfer.fromBranch?.name || getBranchName(transfer.fromBranchId)}</td>
                  <td>{transfer.toBranch?.name || getBranchName(transfer.targetBranchId || transfer.toBranchId)}</td>
                  <td className="transfer-product">
                    {transfer.transferLines?.[0]?.product?.name || `Product ID: ${transfer.transferLines?.[0]?.productId}`}
                  </td>
                  <td>{transfer.transferLines?.[0]?.quantity ?? '—'}</td>
                  <td>{getStatusBadge(transfer.status)}</td>
                  <td>
                    {currentStatus === 'Requested' && (
                      <div className="transfer-actions">
                        <button onClick={() => onAction(transfer.id, 'dispatch')} className="action-button action-dispatch">Dispatch</button>
                        <button onClick={() => onAction(transfer.id, 'cancel')} className="action-button action-cancel">Cancel</button>
                      </div>
                    )}
                    {currentStatus === 'Dispatched' && (
                      <div className="transfer-actions">
                        <button onClick={() => onAction(transfer.id, 'receive')} className="action-button action-receive">Receive</button>
                      </div>
                    )}
                    {(currentStatus === 'Received' || currentStatus === 'Cancelled') && <span className="text-slate-400 font-semibold">--</span>}
                  </td>
                </tr>
              );
            }) : (
              <tr>
                <td colSpan="7" className="empty-state">No transfer records found. Create a new request above to see history.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}

export default TransferListManagement;
