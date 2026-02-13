// <copyright file="AssemblyInfo.cs" company="DefaultCompany">
// Copyright (c) DefaultCompany. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using BovineLabs.Grove.Authoring;
using BovineLabs.Grove.Sample.Authoring.Core;
using Unity.Entities;

[assembly: InternalsVisibleTo("BovineLabs.Grove.Sample.Editor")]
[assembly: InternalsVisibleTo("BovineLabs.Grove.Sample.Tests")]

[assembly: RegisterGenericComponentType(typeof(GraphBakingData<MyGraphAsset>))]
