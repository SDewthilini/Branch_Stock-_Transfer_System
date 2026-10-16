import React from 'react';

// Collects the details needed to submit a stock transfer request.

function NewTransferRequest({
  branches,
  products,
  fromBranchId,
  toBranchId,
  productId,
  quantity,
  onFromBranchChange,
  onToBranchChange,
  onProductChange,
  onQuantityChange,
  onSubmit
}) {
  return (
    <section className="dashboard-card">
      <div className="dashboard-card-heading">
        <span className="section-icon" aria-hidden="true">↗</span>
        <div>
          <h2 className="section-title">New transfer request</h2>
          <p className="section-description">Move products between branch locations.</p>
        </div>
      </div>

      <form onSubmit={onSubmit}>
        <div className="form-fields">
          <div>
            <label className="field-label">From branch</label>
            <select value={fromBranchId} onChange={e => onFromBranchChange(e.target.value)} required className="dashboard-control">
              <option value="">Source Branch</option>
              {branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
            </select>
          </div>

          <div>
            <label className="field-label">To branch</label>
            <select value={toBranchId} onChange={e => onToBranchChange(e.target.value)} required className="dashboard-control">
              <option value="">Destination Branch</option>
              {branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
            </select>
          </div>
        </div>

        <div className="form-fields">
          <div>
            <label className="field-label">Product</label>
            <select value={productId} onChange={e => onProductChange(e.target.value)} required className="dashboard-control">
              <option value="">-- Choose Product --</option>
              {products.map(product => <option key={product.id} value={product.id}>{product.name}</option>)}
            </select>
          </div>

          <div>
            <label className="field-label">Quantity</label>
            <input type="number" value={quantity} onChange={e => onQuantityChange(e.target.value)} min="1" required className="dashboard-control" />
          </div>
        </div>

        <button type="submit" className="primary-button">
          Submit Transfer Request
        </button>
      </form>
    </section>
  );
}

export default NewTransferRequest;
