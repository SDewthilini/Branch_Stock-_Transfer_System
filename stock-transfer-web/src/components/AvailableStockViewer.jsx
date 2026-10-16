import React from 'react';

// Lets the user choose a branch and view its available stock.

function AvailableStockViewer({ branches, selectedBranch, stocks, onBranchChange }) {
  return (
    <section className="dashboard-card">
      <div className="dashboard-card-heading">
        <span className="section-icon" aria-hidden="true">▦</span>
        <div>
          <h2 className="section-title">Available stock</h2>
          <p className="section-description">View inventory by branch location.</p>
        </div>
      </div>

      <div className="mb-4">
        <label className="field-label">Select branch</label>
        <select
          onChange={e => onBranchChange(e.target.value)}
          value={selectedBranch}
          className="dashboard-control"
        >
          <option value="">-- Choose a Branch --</option>
          {branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
        </select>
      </div>

      <div className="table-frame">
        <table className="dashboard-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Available quantity</th>
            </tr>
          </thead>
          <tbody>
            {stocks.length > 0 ? stocks.map((stock, index) => (
              <tr key={stock.id || index}>
                <td>{stock.productName || `Product ID: ${stock.productId}`}</td>
                <td className="stock-quantity">{stock.quantity} units</td>
              </tr>
            )) : (
              <tr>
                <td colSpan="2" className="empty-state">
                  {selectedBranch ? 'No stock items found for this branch.' : 'Please select a branch to view inventory.'}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}

export default AvailableStockViewer;
