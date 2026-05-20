/** @type {import('semantic-release').GlobalConfig} */
export default {
    branches: ['main', { name: 'beta', prerelease: true }],
    plugins: [
        '@semantic-release/commit-analyzer',
        '@semantic-release/release-notes-generator',
        [
            'semantic-release-replace-plugin',
            {
                replacements: [
                    {
                        files: ['Cryptiklemur.RimLogging/BuildInfo.cs'],
                        from: 'Revision = ".*"',
                        to: 'Revision = "${nextRelease.version}"',
                        results: [{ file: 'Cryptiklemur.RimLogging/BuildInfo.cs', hasChanged: true, numMatches: 1, numReplacements: 1 }],
                        countMatches: true,
                    },
                    {
                        files: ['Cryptiklemur.RimLogging/BuildInfo.cs'],
                        from: 'BuildTime = ".*"',
                        to: () => `BuildTime = "${new Date().toISOString()}"`,
                        results: [{ file: 'Cryptiklemur.RimLogging/BuildInfo.cs', hasChanged: true, numMatches: 1, numReplacements: 1 }],
                        countMatches: true,
                    },
                    {
                        files: ['Cryptiklemur.RimLogging/Cryptiklemur.RimLogging.csproj'],
                        from: '<Version>.*</Version>',
                        to: '<Version>${nextRelease.version}</Version>',
                        countMatches: false,
                    },
                    {
                        files: ['Cryptiklemur.RimLogging.UI/Cryptiklemur.RimLogging.UI.csproj'],
                        from: '<Version>.*</Version>',
                        to: '<Version>${nextRelease.version}</Version>',
                        countMatches: false,
                    },
                ],
            },
        ],
        '@semantic-release/github',
        [
            '@semantic-release/git',
            {
                assets: ['Cryptiklemur.RimLogging/BuildInfo.cs'],
                message: 'chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}',
            },
        ],
    ],
};
