using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Serial.GasMixer
{
    public class GasMixture : Gas
    {
        public GasMixture(string name, SimpleGas balanceGas, IEnumerable<GasComponent> nonBalanceComponents)
        {
            FullName = name;
            //Declaration = string.Format("{0,-35}", name);

            BalanceComponent = new GasComponent(balanceGas, 0.0f);
            NonBalanceComponents = nonBalanceComponents.ToList(); //ToDictionary(c => c.Gas.ShortName, c => c);

            UpdateBalanceConcentrationAndKFactor();
        }

        public GasMixture(string name, SimpleGas balanceGas, params GasComponent[] nonBalanceComponents)
            : this(name, balanceGas, nonBalanceComponents as IEnumerable<GasComponent>) { }

        public static GasMixture CreateGasMixtureWithN2Balance(string name, params GasComponent[] nonBalanceComponents) =>
            new GasMixture(name, SimpleGas.CommonGases["N2"], nonBalanceComponents as IEnumerable<GasComponent>);

        public static GasMixture CreateGasMixtureWithN2Balance(string name, SimpleGas simpleGas, float simpleGasConcentration) =>
            new GasMixture(name, SimpleGas.CommonGases["N2"], new GasComponent(simpleGas, simpleGasConcentration));

        //get a gas mixture of a gas with N2
        public static GasMixture CreateGasMixtureWithN2Balance(string commonGas, float commonGasConcentration) =>
            new GasMixture(getFullNameFromGasName(commonGas, commonGasConcentration),
                SimpleGas.CommonGases["N2"], new GasComponent(SimpleGas.CommonGases[commonGas], commonGasConcentration));

        public static GasMixture CreateGasMixtureWithN2Balance(string name, string commonGas, float commonGasConcentration) =>
            new GasMixture(name, SimpleGas.CommonGases["N2"], new GasComponent(SimpleGas.CommonGases[commonGas], commonGasConcentration));

        public static GasMixture CreateGasMixtureWithBalanceOnly(string name, SimpleGas balanceGas) =>
            new GasMixture(name, balanceGas, Enumerable.Empty<GasComponent>());
        public static GasMixture CreateGasMixtureWithBalanceOnly(string commonGas) =>
            new GasMixture(getFullNameFromGasName(commonGas, 1000000.0f), SimpleGas.CommonGases[commonGas], Enumerable.Empty<GasComponent>());

        private static string getFullNameFromGasName(string gasName, float concentrationInPpm)
        {
            return concentrationInPpm > 5000.0f ?
                    $"{gasName} {concentrationInPpm / 10000.0:0.0#}%" :
                    $"{gasName} {concentrationInPpm:0.#} ppm";
        }

        /// <summary>
        /// There must be at least one balance component.
        /// </summary>
        public GasComponent BalanceComponent { get; }

        /// <summary>
        /// Non-balance components are optional and include all components except the BalanceComponent.
        /// </summary>
        public List<GasComponent> NonBalanceComponents { get; protected set; }


        public void UpdateBalanceConcentrationAndKFactor()
        {
            if (NonBalanceComponents.Any())
            {
                BalanceComponent.Concentration = 1e6f - NonBalanceComponents.Select(c => c.Concentration).Sum();
                KFactor = NonBalanceComponents.Select(c => c.Concentration * 1e-6f * c.Gas.KFactor).Sum() +
                    BalanceComponent.Concentration * 1e-6f * BalanceComponent.Gas.KFactor;
            }
            else
            {
                BalanceComponent.Concentration = 1e6f;
                KFactor = BalanceComponent.Gas.KFactor;
            }

        }

    }


}
