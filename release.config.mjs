/** @type {import('semantic-release').GlobalConfig} */
export default {
    branches: ['main', { name: 'beta', prerelease: true }],
    plugins: [
        [
            '@semantic-release/commit-analyzer',
            {
                releaseRules: [
                    { type: 'refactor', release: 'patch' },
                    { type: 'style', release: 'patch' },
                    { type: 'ci', release: 'patch' },
                ],
            },
        ],
        '@semantic-release/release-notes-generator',
        [
            'semantic-release-replace-plugin',
            {
                replacements: [
                    {
                        files: ['CryptikLemur.RimLogging/BuildInfo.cs'],
                        from: 'Revision = ".*"',
                        to: 'Revision = "${nextRelease.version}"',
                        results: [{ file: 'CryptikLemur.RimLogging/BuildInfo.cs', hasChanged: true, numMatches: 1, numReplacements: 1 }],
                        countMatches: true,
                    },
                    {
                        files: ['CryptikLemur.RimLogging/BuildInfo.cs'],
                        from: 'BuildTime = ".*"',
                        to: () => `BuildTime = "${new Date().toISOString()}"`,
                        results: [{ file: 'CryptikLemur.RimLogging/BuildInfo.cs', hasChanged: true, numMatches: 1, numReplacements: 1 }],
                        countMatches: true,
                    },
                    {
                        files: ['CryptikLemur.RimLogging/CryptikLemur.RimLogging.csproj'],
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
                assets: ['CryptikLemur.RimLogging/BuildInfo.cs'],
                message: 'chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}',
            },
        ],
    ],
};
