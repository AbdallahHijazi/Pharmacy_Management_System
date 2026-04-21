using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Organization;
using Pharmacy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Finance
{
    public class Payment : BaseEntity
    {
        public PaymentPartyType PartyType { get; set; }
        public Guid PartyId { get; set; }                    // CustomerId أو SupplierId
        public Guid? InvoiceId { get; set; }
        public ReferenceType InvoiceType { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Guid BranchId { get; set; }

        public Branch Branch { get; set; } = null!;
    }
}
