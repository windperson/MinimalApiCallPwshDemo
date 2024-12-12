#region Script Requirement settings
#Requires -Version 7
#Requires -Module @{ ModuleName='Pester'; ModuleVersion="5.6.1"}
#endregion

BeforeAll {
    Import-Module "$PSScriptRoot\..\..\..\src\DemoWebApi\PwshScripts\DuplicateFilesFinder.psm1"
}

Describe "Verify Get-DuplicateFile function" -Tag "DuplicateFileFinder" {
    Context "When two directories have duplicate files" {
        BeforeAll {
            #region test source folder setup
            function CreateTestFiles {
                param($sourcePath)

                if (-not(Test-Path -Path $sourcePath)) {
                    New-Item -Path $sourcePath -ItemType Directory -ErrorAction Stop
                }
                "Test File 1" > $($sourcePath + "\file1.txt")
                "Test File 2" > $($sourcePath + "\file2.txt")
                "Test File 3" > $($sourcePath + "\file3.txt")
                $subFolderAlpha = Join-Path $sourcePath -ChildPath "Alpha"
                if (-not (Test-Path -Path $subFolderAlpha)) {
                    New-Item -Path $subFolderAlpha -ItemType Directory -ErrorAction Stop
                }
                "Test File alpha 1" > $($subFolderAlpha + "\file1.txt")
                "Test File alpha 2" > $($subFolderAlpha + "\file2.txt")
                $subFolderAlphaAndOne = Join-Path $subFolderAlpha -ChildPath "One"
                if (-not (Test-Path -Path $subFolderAlphaAndOne)) {
                    New-Item -Path $subFolderAlphaAndOne -ItemType Directory -ErrorAction Stop
                }
                "Test File alpha one 1" > $($subFolderAlphaAndOne + "\file1.txt")
                "Test File alpha one 2" > $($subFolderAlphaAndOne + "\file2.txt")
                $subFolderBeta = Join-Path $sourcePath -ChildPath "Beta"
                if (-not (Test-Path -Path $subFolderBeta)) {
                    New-Item -Path $subFolderBeta -ItemType Directory -ErrorAction Stop
                }
                "Test File beta 1" > $($subFolderBeta + "\file1.txt")
                "Test File beta 2" > $($subFolderBeta + "\file2.txt")
                $subFolderGamma = Join-Path $sourcePath -ChildPath "Gamma"
                if (-not (Test-Path -Path $subFolderGamma)) {
                    New-Item -Path $subFolderGamma -ItemType Directory -ErrorAction Stop
                }
                "Test File gamma 1" > $($subFolderGamma + "\file1.txt")
            }
            $sourcePath = "$TestDrive\Source"
            CreateTestFiles -sourcePath $sourcePath
            #endregion
        }

        It "Should return empty array when one of the directories is empty" {
            # Arrange
            #region test compare folder setup
            $comparePath = "$TestDrive\Compare"
            if (-not(Test-Path -Path $comparePath)) {
                New-Item -Path $comparePath -ItemType Directory -ErrorAction Stop
            }
            #endregion

            # Act
            $result = Get-DuplicateFile -SourcePath $sourcePath -ComparePath $comparePath

            # Assert
            # Be sure to unroll the result array using -NoEnumerate
            Write-Output $result -NoEnumerate | Should -BeOfType [Object[]]
            # Or use the following -isnot operator to check the type
            if ($result -isnot [Object[]]) {
                throw "Expected result to be of type [Object[]]"
            }
            $result | Should -BeNullOrEmpty
        }

        It "Should return empty array when both source and compare path directories are empty"{
            # Arrange
            $sourcePath = "$TestDrive\Source"
            if (-not(Test-Path -Path $sourcePath)) {
                New-Item -Path $sourcePath -ItemType Directory -ErrorAction Stop
            }
            $comparePath = "$TestDrive\Compare"
            if (-not(Test-Path -Path $comparePath)) {
                New-Item -Path $comparePath -ItemType Directory -ErrorAction Stop
            }

            # Act
            $result = Get-DuplicateFile -SourcePath $sourcePath -ComparePath $comparePath

            # Assert
            Write-Output $result -NoEnumerate | Should -BeOfType [Object[]]
            $result | Should -BeNullOrEmpty

        }

        It "return vaild result when there's identical files" {
            # Arrange
            #region test compare folder setup
            $comparePath = "$TestDrive\Compare"
            if (-not(Test-Path -Path $comparePath)) {
                New-Item -Path $comparePath -ItemType Directory -ErrorAction Stop
            }
            $subFolderAlpha = Join-Path $comparePath -ChildPath "Alpha"
            if (-not (Test-Path -Path $subFolderAlpha)) {
                New-Item -Path $subFolderAlpha -ItemType Directory -ErrorAction Stop
            }
            $subFolderAlphaAndOne = Join-Path $subFolderAlpha -ChildPath "One"
            if (-not (Test-Path -Path $subFolderAlphaAndOne)) {
                New-Item -Path $subFolderAlphaAndOne -ItemType Directory -ErrorAction Stop
            }
            "Test File alpha one 1" > $($subFolderAlphaAndOne + "\file1.txt")
            $subFolderBeta = Join-Path $comparePath -ChildPath "Beta"
            if (-not (Test-Path -Path $subFolderBeta)) {
                New-Item -Path $subFolderBeta -ItemType Directory -ErrorAction Stop
            }
            "Test File beta 2" > $($subFolderBeta + "\file2.txt")
            #endregion

            # Act
            $result1 = Get-DuplicateFile -SourcePath $sourcePath -ComparePath $comparePath
            $result2 = Get-DuplicateFile -SourcePath $comparePath -ComparePath $sourcePath


            # Assert
            $result1 | Should -Not -BeNullOrEmpty
            $result1 | Should -HaveCount 2
            $result1[0].FilePath1.FullName | Should -Be "$sourcePath\Alpha\One\file1.txt"
            $result1[0].FilePath2.FullName | Should -Be "$comparePath\Alpha\One\file1.txt"
            $result1[1].FilePath1.FullName | Should -Be "$sourcePath\Beta\file2.txt"
            $result1[1].FilePath2.FullName | Should -Be "$comparePath\Beta\file2.txt"

            $result2 | Should -Not -BeNullOrEmpty
            $result2 | Should -HaveCount 2
            $result2[0].FilePath1.FullName | Should -Be "$comparePath\Alpha\One\file1.txt"
            $result2[0].FilePath2.FullName | Should -Be "$sourcePath\Alpha\One\file1.txt"
            $result2[1].FilePath1.FullName | Should -Be "$comparePath\Beta\file2.txt"
            $result2[1].FilePath2.FullName | Should -Be "$sourcePath\Beta\file2.txt"
        }
    }

}
#endregion
