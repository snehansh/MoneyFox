﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using MoneyManager.Foundation;

namespace MoneyFox.Foundation.Model
{
    /// <summary>
    ///     Databasemodel for payments. Includes expenses, income and transfers.
    ///     Databasetable: Payments
    /// </summary>
    public class Payment : INotifyPropertyChanged
    {
        private Category category;

        private Account chargedAccount;

        private RecurringPayment recurringPayment;

        private Account targetAccount;

        /// <summary>
        ///     Primary Key. Is generated by the database on insert.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     In case it's a expense or transfer the foreign key to the <see cref="Account" /> who will be charged.
        ///     In case it's an income the  foreign key to the <see cref="Account" /> who will be credited.
        /// </summary>
        public int ChargedAccountId { get; set; }

        /// <summary>
        ///     Foreign key to the account who will be credited by a transfer.
        ///     Not used for the other payment types.
        /// </summary>
        public int TargetAccountId { get; set; }

        /// <summary>
        ///     Foreign key to the <see cref="Category" /> for this payment
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        ///     Date when this payment will be executed.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     Amount of the payment. Has to be >= 0. If the amount is charged or not is based on the payment type.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        ///     Indicates if this payment was already executed and the amount already credited or charged to the respective
        ///     account.
        /// </summary>
        public bool IsCleared { get; set; }

        /// <summary>
        ///     Type of the payment. This is the int of the Enum <see cref="PaymentType" />.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        ///     Additional notes to the payment.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        ///     Indicates if the payment will be repeated or if it's a uniquie payment.
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        ///     Foreign key to the <see cref="RecurringPayment" /> if it's recurring.
        /// </summary>
        public int RecurringPaymentId { get; set; }

        /// <summary>
        ///     In case it's a expense or transfer the account who will be charged.
        ///     In case it's an income the account who will be credited.
        /// </summary>
        [NotMapped]
        public Account ChargedAccount
        {
            get { return chargedAccount; }
            set
            {
                if (chargedAccount != value)
                {
                    chargedAccount = value;
                    ChargedAccountId = value.Id;
                }
            }
        }

        /// <summary>
        ///     The <see cref="Account" /> who will be credited by a transfer.
        ///     Not used for the other payment types.
        /// </summary>
        [NotMapped]
        public Account TargetAccount
        {
            get { return targetAccount; }
            set
            {
                if (targetAccount != value)
                {
                    targetAccount = value;
                    TargetAccountId = value.Id;
                }
            }
        }

        /// <summary>
        ///     The <see cref="Category" /> for this payment
        /// </summary>
        [NotMapped]
        public Category Category
        {
            get { return category; }
            set
            {
                if (category != value)
                {
                    category = value;
                    CategoryId = value?.Id;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The <see cref="RecurringPayment" /> if it's recurring.
        /// </summary>
        [NotMapped]
        public RecurringPayment RecurringPayment
        {
            get { return recurringPayment; }
            set
            {
                if (recurringPayment != value)
                {
                    recurringPayment = value;
                    RecurringPaymentId = value.Id;
                }
            }
        }

        /// <summary>
        ///     Checks if the payment is ready to clear based on the date of
        ///     the payment and the current date.
        /// </summary>
        [NotMapped]
        public bool ClearPaymentNow => Date.Date <= DateTime.Now.Date;

        /// <summary>
        ///     This is a shortcut to access if the payment is a transfer or not.
        /// </summary>
        [NotMapped]
        public bool IsTransfer => Type == (int) PaymentType.Transfer;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}