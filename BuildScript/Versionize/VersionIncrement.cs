
using System;
using System.Collections.Generic;

namespace BuildScript
{

    public class VersionIncrementStrategy
    {
        private readonly VersionImpact versionImpact;

        private VersionIncrementStrategy(VersionImpact versionImpact)
        {
            this.versionImpact = versionImpact;
        }

        public Version NextVersion(Version version, bool ignoreInsignificant = false)
        {
            switch (versionImpact)
            {
                case VersionImpact.Patch:
                    return new Version(version.Major, version.Minor, version.Build + 1);
                case VersionImpact.Minor:
                    return new Version(version.Major, version.Minor + 1, 0);
                case VersionImpact.Major:
                    return new Version(version.Major + 1, 0, 0);
                case VersionImpact.None:
                    var buildVersion = ignoreInsignificant ? version.Build : version.Build + 1;
                    return new Version(version.Major, version.Minor, buildVersion);
                default:
                    throw new InvalidOperationException($"Version impact of {versionImpact} cannot be handled");
            }
        }

        public static VersionIncrementStrategy CreateFrom(IEnumerable<ConventionalCommit> conventionalCommits)
        {
            // TODO: Quick and dirty implementation - Conventions? Better comparison?
            var versionImpact = VersionImpact.None;

            foreach (var conventionalCommit in conventionalCommits)
            {
                if (!string.IsNullOrWhiteSpace(conventionalCommit.Type))
                {
                    if (conventionalCommit.IsFix)
                    {
                        versionImpact = MaxVersionImpact(versionImpact, VersionImpact.Patch);
                    }
                    else if (conventionalCommit.IsFeature)
                    {
                        versionImpact = MaxVersionImpact(versionImpact, VersionImpact.Minor);
                    }
                }

                if (conventionalCommit.IsBreakingChange)
                {
                    versionImpact = MaxVersionImpact(versionImpact, VersionImpact.Major);
                }
            }

            return new VersionIncrementStrategy(versionImpact);
        }

        private static VersionImpact MaxVersionImpact(VersionImpact impact1, VersionImpact impact2)
        {
            return (VersionImpact)Math.Max((int)impact1, (int)impact2);
        }
    }

    public enum VersionImpact
    {
        None = 0,

        Patch = 1,
        Minor = 2,
        Major = 3,
    }
}
