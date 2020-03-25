/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Brokerages.Bybit
{
    /// <summary>
    /// Provides the mapping between Lean symbols and Bybit symbols.
    /// </summary>
    public class BybitSymbolMapper : ISymbolMapper
    {
        /// <summary>
        /// Symbols that are both active and delisted
        /// </summary>
        public static List<Symbol> KnownSymbols
        {
            get
            {
                var symbols = new List<Symbol>();
                var mapper = new BybitSymbolMapper();
                foreach (var tp in KnownSymbolStrings)
                {
                    symbols.Add(mapper.GetLeanSymbol(tp, mapper.GetBrokerageSecurityType(tp), Market.Bybit));
                }
                return symbols;
            }
        }

        /// <summary>
        /// The list of known Bybit symbols.
        /// </summary>
        public static readonly HashSet<string> KnownSymbolStrings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BTCUSD","ETHUSD","EOSUSD","XRPUSD"
        };

        /// <summary>
        /// The list of delisted/invalid Bybit symbols.
        /// </summary>
        public static HashSet<string> DelistedSymbolStrings = new HashSet<string> { };

        /// <summary>
        /// The list of active Bybit symbols.
        /// </summary>
        public static List<string> ActiveSymbolStrings =
            KnownSymbolStrings
                .Where(x => !DelistedSymbolStrings.Contains(x))
                .ToList();

        /// <summary>
        /// The list of known Bybit currencies.
        /// </summary>
        private static readonly HashSet<string> KnownCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "USD"
        };

        /// <summary>
        /// Converts a Lean symbol instance to an Bybit symbol
        /// </summary>
        /// <param name="symbol">A Lean symbol instance</param>
        /// <returns>The Bybit symbol</returns>
        public string GetBrokerageSymbol(Symbol symbol)
        {
            if (symbol == null || string.IsNullOrWhiteSpace(symbol.Value))
                throw new ArgumentException("Invalid symbol: " + (symbol == null ? "null" : symbol.ToString()));

            if (symbol.ID.SecurityType != SecurityType.Crypto)
                throw new ArgumentException("Invalid security type: " + symbol.ID.SecurityType);

            var brokerageSymbol = ConvertLeanSymbolToBybitSymbol(symbol.Value);

            if (!IsKnownBrokerageSymbol(brokerageSymbol))
                throw new ArgumentException("Unknown symbol: " + symbol.Value);

            return brokerageSymbol;
        }

        /// <summary>
        /// Converts an Bybit symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The Bybit symbol</param>
        /// <param name="securityType">The security type</param>
        /// <param name="market">The market</param>
        /// <param name="expirationDate">Expiration date of the security(if applicable)</param>
        /// <param name="strike">The strike of the security (if applicable)</param>
        /// <param name="optionRight">The option right of the security (if applicable)</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol(string brokerageSymbol, SecurityType securityType, string market, DateTime expirationDate = default(DateTime), decimal strike = 0, OptionRight optionRight = 0)
        {
            if (string.IsNullOrWhiteSpace(brokerageSymbol))
                throw new ArgumentException($"Invalid Bybit symbol: {brokerageSymbol}");

            if (!IsKnownBrokerageSymbol(brokerageSymbol))
                throw new ArgumentException($"Unknown Bybit symbol: {brokerageSymbol}");

            if (securityType != SecurityType.Crypto)
                throw new ArgumentException($"Invalid security type: {securityType}");

            if (market != Market.Bybit)
                throw new ArgumentException($"Invalid market: {market}");

            return Symbol.Create(ConvertBybitSymbolToLeanSymbol(brokerageSymbol), GetBrokerageSecurityType(brokerageSymbol), Market.Bybit);
        }

        /// <summary>
        /// Converts an Bybit symbol to a Lean symbol instance
        /// </summary>
        /// <param name="brokerageSymbol">The Bybit symbol</param>
        /// <returns>A new Lean Symbol instance</returns>
        public Symbol GetLeanSymbol(string brokerageSymbol)
        {
            var securityType = GetBrokerageSecurityType(brokerageSymbol);
            return GetLeanSymbol(brokerageSymbol, securityType, Market.Bybit);
        }

        /// <summary>
        /// Returns the security type for an Bybit symbol
        /// </summary>
        /// <param name="brokerageSymbol">The Bybit symbol</param>
        /// <returns>The security type</returns>
        public SecurityType GetBrokerageSecurityType(string brokerageSymbol)
        {
            if (string.IsNullOrWhiteSpace(brokerageSymbol))
                throw new ArgumentException($"Invalid Bybit symbol: {brokerageSymbol}");

            if (!IsKnownBrokerageSymbol(brokerageSymbol))
                throw new ArgumentException($"Unknown Bybit symbol: {brokerageSymbol}");

            return SecurityType.Crypto;
        }

        /// <summary>
        /// Returns the security type for a Lean symbol
        /// </summary>
        /// <param name="leanSymbol">The Lean symbol</param>
        /// <returns>The security type</returns>
        public SecurityType GetLeanSecurityType(string leanSymbol)
        {
            return GetBrokerageSecurityType(ConvertLeanSymbolToBybitSymbol(leanSymbol));
        }

        /// <summary>
        /// Checks if the symbol is supported by Bybit
        /// </summary>
        /// <param name="brokerageSymbol">The Bybit symbol</param>
        /// <returns>True if Bybit supports the symbol</returns>
        public bool IsKnownBrokerageSymbol(string brokerageSymbol)
        {
            if (string.IsNullOrWhiteSpace(brokerageSymbol))
                return false;

            return KnownSymbolStrings.Contains(brokerageSymbol);
        }

        /// <summary>
        /// Checks if the currency is supported by Bybit
        /// </summary>
        /// <returns>True if Bybit supports the currency</returns>
        public bool IsKnownFiatCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return false;

            return KnownCurrencies.Contains(currency);
        }

        /// <summary>
        /// Checks if the symbol is supported by Bybit
        /// </summary>
        /// <param name="symbol">The Lean symbol</param>
        /// <returns>True if Bybit supports the symbol</returns>
        public bool IsKnownLeanSymbol(Symbol symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol?.Value) || symbol?.Value.Length <= 3)
                return false;

            var bybitSymbol = ConvertLeanSymbolToBybitSymbol(symbol.Value);

            return IsKnownBrokerageSymbol(bybitSymbol) && GetBrokerageSecurityType(bybitSymbol) == symbol.ID.SecurityType;
        }

        /// <summary>
        /// Converts an Bybit symbol to a Lean symbol string
        /// </summary>
        private static string ConvertBybitSymbolToLeanSymbol(string bybitSymbol)
        {
            if (string.IsNullOrWhiteSpace(bybitSymbol))
                throw new ArgumentException($"Invalid Bybit symbol: {bybitSymbol}");

            // return as it is due to Bybit has similar Symbol format
            return bybitSymbol.ToUpperInvariant();
        }

        /// <summary>
        /// Converts a Lean symbol string to an Bybit symbol
        /// </summary>
        private static string ConvertLeanSymbolToBybitSymbol(string leanSymbol)
        {
            if (string.IsNullOrWhiteSpace(leanSymbol))
                throw new ArgumentException($"Invalid Lean symbol: {leanSymbol}");

            // return as it is due to Bybit has similar Symbol format
            return leanSymbol.ToUpperInvariant();
        }
    }
}
