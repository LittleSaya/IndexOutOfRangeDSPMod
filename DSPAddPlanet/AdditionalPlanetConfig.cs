using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPAddPlanet
{
    struct AdditionalPlanetConfig
    {
        public int Index { get; set; }

        public int OrbitAround { get; set; }

        public int OrbitIndex { get; set; }

        public int Number { get; set; }

        public bool GasGiant { get; set; }

        public int InfoSeed { get; set; }

        public int GenSeed { get; set; }

        public float Radius { get; set; }

        public float OrbitalPeriod { get; set; }

        public float RotationPeriod { get; set; }

        public bool IsTidalLocked { get; set; }

        public float OrbitInclination { get; set; }

        public float Obliquity { get; set; }

        public bool DontGenerateVein { get; set; }

        override public string ToString ()
        {
            return $"Index: {Index}, OrbitAround: {OrbitAround}, OrbitIndex: {OrbitIndex}, Number: {Number}, GasGiant: {GasGiant}, InfoSeed: {InfoSeed}, GenSeed: {GenSeed}, Radius: {Radius}, OrbitalPeriod: {OrbitalPeriod}, RotationPeriod: {RotationPeriod}, IsTidalLocked: {IsTidalLocked}, OrbitInclination: {OrbitInclination}, Obliquity: {Obliquity}, DontGenerateVein: {DontGenerateVein}";
        }
    }
}
